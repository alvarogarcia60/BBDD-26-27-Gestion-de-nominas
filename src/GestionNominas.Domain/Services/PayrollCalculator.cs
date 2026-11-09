using System;
using System.Collections.Generic;
using System.Linq;
using GestionNominas.Domain.Entities;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de cálculo y generación masiva de nóminas mensuales.
    /// Implementa todas las reglas de negocio definidas en el enunciado del proyecto.
    /// </summary>
    public class PayrollCalculator
    {
        // ─────────────────────────────────────────────────────────────────────
        // Clase de resultado que envuelve el resultado de la operación
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Encapsula el resultado de la generación masiva de nóminas.
        /// </summary>
        public class PayrollResult
        {
            /// <summary>Indica si la operación fue exitosa.</summary>
            public bool Success { get; set; }

            /// <summary>Mensaje descriptivo del resultado (éxito o motivo del fallo).</summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>Número de empleados para los que se generó nómina.</summary>
            public int ProcessedEmployeesCount { get; set; }

            /// <summary>Lista de nóminas generadas durante la operación.</summary>
            public List<MonthlyPayment> GeneratedPayments { get; set; } = new List<MonthlyPayment>();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Método principal de generación de nóminas
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Genera todas las nóminas mensuales para el año y mes indicados.
        /// </summary>
        /// <param name="year">Año del periodo a liquidar.</param>
        /// <param name="month">Mes del periodo a liquidar (1–12).</param>
        /// <returns>Un <see cref="PayrollResult"/> con el detalle de la operación.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el mes no está entre 1 y 12.</exception>
        /// <exception cref="InvalidOperationException">
        ///   Si el periodo corresponde al mes actual o a un mes futuro,
        ///   o si las nóminas del periodo ya fueron generadas previamente.
        /// </exception>
        public PayrollResult GenerateMonthlyPayroll(int year, int month)
        {
            // ── 1. Validación de parámetros básicos ──────────────────────────
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month),
                    "El mes debe estar comprendido entre 1 y 12.");

            if (year < 2000 || year > DateTime.Today.Year + 10)
                throw new ArgumentOutOfRangeException(nameof(year),
                    $"El año '{year}' no es un valor razonable.");

            // ── 2. El periodo debe ser estrictamente anterior al mes actual ──
            DateTime periodStart       = new DateTime(year, month, 1);
            DateTime today             = DateTime.Today;
            DateTime currentMonthStart = new DateTime(today.Year, today.Month, 1);

            if (periodStart >= currentMonthStart)
                throw new InvalidOperationException(
                    $"No se pueden generar nóminas para el periodo {year}/{month:D2}: " +
                    "el mes debe estar completamente cerrado (anterior al mes actual).");

            // ── 3. Verificar si el periodo ya fue generado ───────────────────
            if (MonthlyPayment.PeriodAlreadyGenerated(year, month))
                throw new InvalidOperationException(
                    $"Las nóminas del periodo {year}/{month:D2} ya fueron generadas. " +
                    "No se ha modificado la información existente.");

            // ── 4. Variables de contexto del mes ────────────────────────────
            int      daysInMonth     = DateTime.DaysInMonth(year, month);
            DateTime firstDayOfMonth = periodStart;
            DateTime lastDayOfMonth  = new DateTime(year, month, daysInMonth);

            // ── 5. Lectura masiva de datos (evita N+1 queries) ───────────────
            List<Employee> allEmployees = Employee.ReadAll();
            List<Position> allPositions = Position.ReadAll();

            // Agrupar puestos por empleado para O(1) lookup
            Dictionary<int, List<Position>> empPositions =
                allPositions
                    .GroupBy(p => p.Employee.IdEmployee)
                    .ToDictionary(g => g.Key, g => g.ToList());

            // ── 6. Preparar resultado y acceso al broker ─────────────────────
            PayrollResult result = new PayrollResult();
            DBBroker      broker = DBBroker.GetInstance();

            // ── 7. Transacción que envuelve todo el bucle ────────────────────
            broker.BeginTransaction();

            try
            {
                foreach (Employee employee in allEmployees)
                {
                    // Obtener puestos de este empleado (puede no tener ninguno)
                    if (!empPositions.TryGetValue(employee.IdEmployee, out List<Position>? positions))
                        positions = new List<Position>();

                    // ── 7a. Filtrar puestos activos durante el mes ───────────
                    // Un puesto es "activo en el mes" si empezó antes del último día
                    // del mes Y aún no había terminado al inicio del mes.
                    var activeWithDays = positions
                        .Where(p =>
                            p.StartDate <= lastDayOfMonth &&
                            (p.EndDate == null || p.EndDate >= firstDayOfMonth))
                        .Select(p => new
                        {
                            Position   = p,
                            DaysWorked = CalculateDaysWorkedInMonth(p, firstDayOfMonth, lastDayOfMonth)
                        })
                        .Where(x => x.DaysWorked > 0)
                        .ToList();

                    // Si el empleado no trabajó ningún día en el mes, se omite
                    if (!activeWithDays.Any())
                        continue;

                    // ── 7b. Días históricos totales → Trienios ───────────────
                    // Se acumulan días de TODOS los puestos cuya fecha de inicio
                    // no supere el último día del mes (contratos futuros ignorados).
                    int historicalDays = positions
                        .Where(p => p.StartDate <= lastDayOfMonth)
                        .Sum(p =>
                        {
                            // Fecha efectiva de fin: la menor entre EndDate y lastDayOfMonth
                            DateTime effectiveEnd =
                                (p.EndDate.HasValue && p.EndDate.Value < lastDayOfMonth)
                                ? p.EndDate.Value
                                : lastDayOfMonth;

                            // +1 para contar el día de inicio inclusive
                            return (effectiveEnd - p.StartDate).Days + 1;
                        });

                    // Trienios = bloques completos de 1095 días trabajados
                    int trienios = historicalDays / 1095;

                    // ── 7c. Una nómina por cada oficina en la que trabajó en el mes ──
                    // Criterio acordado con el profesor: si el empleado cambia de oficina,
                    // tiene una nómina por oficina con los días trabajados en cada una.
                    // Se insertan por orden de llegada a cada oficina: los informes
                    // dependen de ese orden (ver MonthlyPayment.PaymentOfficeSql).
                    var officeGroups = activeWithDays
                        .GroupBy(x => x.Position.Office.IdOffice)
                        .OrderBy(g => g.Min(x => x.Position.StartDate));

                    foreach (var officeGroup in officeGroups)
                    {
                        Office office = officeGroup.First().Position.Office;

                        // Salario base proporcional a los días de cada puesto en esta oficina
                        decimal baseSalary = officeGroup.Sum(x =>
                            (x.Position.YearSalary / 12m) *
                            ((decimal)x.DaysWorked / (decimal)daysInMonth));

                        // Bonus de la oficina multiplicado por los trienios completos (en %)
                        decimal bonusRate = (decimal)trienios * office.ThreeYearBonus;

                        decimal gross = baseSalary * (1m + bonusRate / 100m);
                        decimal net   = gross * (1m - office.Country.TaxBase / 100m);

                        // ── 7d. Insertar nómina en la BD (dentro de la transacción)
                        MonthlyPayment payment = new MonthlyPayment
                        {
                            Employee     = employee,
                            Year         = year,
                            Month        = month,
                            SalaryGross  = Math.Round(gross, 2, MidpointRounding.AwayFromZero),
                            SalaryNet    = Math.Round(net, 2, MidpointRounding.AwayFromZero),
                            BonusApplied = Math.Round(bonusRate, 2, MidpointRounding.AwayFromZero)
                        };

                        payment.Insert();
                        result.GeneratedPayments.Add(payment);
                    }
                }

                // ── 8. Todo correcto: confirmar transacción ──────────────────
                broker.CommitTransaction();
                result.Success                = true;
                result.ProcessedEmployeesCount =
                    result.GeneratedPayments.Select(p => p.Employee.IdEmployee).Distinct().Count();
                result.Message =
                    $"Se han generado {result.GeneratedPayments.Count} nómina(s) del periodo {year}/{month:D2} " +
                    $"para {result.ProcessedEmployeesCount} empleado(s).";
            }
            catch (Exception ex)
            {
                // ── 9. Cualquier fallo: revertir para evitar datos parciales ──
                try { broker.RollbackTransaction(); }
                catch { /* absorber error del rollback para no enmascarar el original */ }

                result.Success = false;
                result.Message =
                    $"Error al generar nóminas del periodo {year}/{month:D2}. " +
                    $"Se revirtieron todos los cambios. Detalle: {ex.Message}";
                result.GeneratedPayments.Clear();
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Métodos auxiliares privados
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calcula los días que un puesto estuvo activo dentro de un mes concreto.
        /// </summary>
        /// <param name="position">Puesto a evaluar.</param>
        /// <param name="firstDay">Primer día del mes.</param>
        /// <param name="lastDay">Último día del mes.</param>
        /// <returns>Días trabajados en el mes (0 si el puesto no solapó el mes).</returns>
        private static int CalculateDaysWorkedInMonth(
            Position position,
            DateTime firstDay,
            DateTime lastDay)
        {
            // Inicio efectivo: el mayor entre el inicio del puesto y el primer día del mes
            DateTime effectiveStart = position.StartDate > firstDay
                ? position.StartDate
                : firstDay;

            // Fin efectivo: el menor entre el fin del puesto (o infinito si abierto) y el último día del mes
            DateTime effectiveEnd =
                (!position.EndDate.HasValue || position.EndDate.Value > lastDay)
                ? lastDay
                : position.EndDate.Value;

            int days = (effectiveEnd - effectiveStart).Days + 1;
            return days > 0 ? days : 0;
        }
    }
}
