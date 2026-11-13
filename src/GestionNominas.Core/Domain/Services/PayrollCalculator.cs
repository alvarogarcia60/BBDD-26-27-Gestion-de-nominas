using System;
using System.Collections.Generic;
using System.Linq;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Genera las nóminas de todos los empleados para un mes, aplicando las reglas del enunciado.
    /// </summary>
    public class PayrollCalculator
    {
        /// <summary>
        /// Resultado de una generación de nóminas.
        /// </summary>
        public class PayrollResult
        {
            /// <summary>Indica si las nóminas se han guardado.</summary>
            public bool Success { get; set; }

            /// <summary>Mensaje para el usuario (resultado o motivo del fallo).</summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>Número de empleados con nómina.</summary>
            public int ProcessedEmployeesCount { get; set; }

            /// <summary>Nóminas generadas.</summary>
            public List<MonthlyPayment> GeneratedPayments { get; set; } = new List<MonthlyPayment>();
        }

        /// <summary>
        /// Genera y guarda las nóminas del año y mes indicados.
        /// Lanza una excepción si el mes no es anterior al actual o si ya se generó.
        /// </summary>
        public PayrollResult GenerateMonthlyPayroll(int year, int month)
        {
            // 1. Parámetros válidos
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month),
                    "El mes debe estar comprendido entre 1 y 12.");

            if (year < 2000 || year > DateTime.Today.Year + 10)
                throw new ArgumentOutOfRangeException(nameof(year),
                    $"El año '{year}' no es un valor razonable.");

            // 2. Solo meses anteriores al actual
            DateTime periodStart       = new DateTime(year, month, 1);
            DateTime today             = DateTime.Today;
            DateTime currentMonthStart = new DateTime(today.Year, today.Month, 1);

            if (periodStart >= currentMonthStart)
                throw new InvalidOperationException(
                    $"No se pueden generar nóminas para el periodo {year}/{month:D2}: " +
                    "el mes debe estar completamente cerrado (anterior al mes actual).");

            // 3. Un mes ya generado no se vuelve a generar ni se modifica
            if (MonthlyPayment.PeriodAlreadyGenerated(year, month))
                throw new InvalidOperationException(
                    $"Las nóminas del periodo {year}/{month:D2} ya fueron generadas. " +
                    "No se ha modificado la información existente.");

            // 4. Datos del mes
            int      daysInMonth     = DateTime.DaysInMonth(year, month);
            DateTime firstDayOfMonth = periodStart;
            DateTime lastDayOfMonth  = new DateTime(year, month, daysInMonth);

            // 5. Empleados y puestos, leídos una sola vez y agrupados por empleado
            List<Employee> allEmployees = Employee.ReadAll();
            List<Position> allPositions = Position.ReadAll();

            Dictionary<int, List<Position>> empPositions =
                allPositions
                    .GroupBy(p => p.Employee.IdEmployee)
                    .ToDictionary(g => g.Key, g => g.ToList());

            PayrollResult result = new PayrollResult();

            try
            {
                foreach (Employee employee in allEmployees)
                {
                    if (!empPositions.TryGetValue(employee.IdEmployee, out List<Position>? positions))
                        positions = new List<Position>();

                    // 6. Puestos activos en el mes y días trabajados en cada uno
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

                    // Sin ningún día trabajado en el mes no hay nómina
                    if (!activeWithDays.Any())
                        continue;

                    // 7. Trienios: días trabajados en cualquier puesto hasta el último día del mes
                    //    (los días posteriores al mes no cuentan)
                    int historicalDays = positions
                        .Where(p => p.StartDate <= lastDayOfMonth)
                        .Sum(p =>
                        {
                            DateTime effectiveEnd =
                                (p.EndDate.HasValue && p.EndDate.Value < lastDayOfMonth)
                                ? p.EndDate.Value
                                : lastDayOfMonth;

                            return (effectiveEnd - p.StartDate).Days + 1;
                        });

                    int trienios = historicalDays / 1095;

                    // 8. Una nómina por cada oficina en la que trabajó en el mes, por orden de llegada.
                    //    Ese orden permite saber después la oficina de cada nómina
                    //    (ver MonthlyPaymentDAO.PaymentOfficeSql).
                    var officeGroups = activeWithDays
                        .GroupBy(x => x.Position.Office.IdOffice)
                        .OrderBy(g => g.Min(x => x.Position.StartDate));

                    foreach (var officeGroup in officeGroups)
                    {
                        Office office = officeGroup.First().Position.Office;

                        // Salario proporcional a los días de cada puesto en esta oficina
                        decimal baseSalary = officeGroup.Sum(x =>
                            (x.Position.YearSalary / 12m) *
                            ((decimal)x.DaysWorked / (decimal)daysInMonth));

                        // Bonus de la oficina multiplicado por los trienios (en %)
                        decimal bonusRate = (decimal)trienios * office.ThreeYearBonus;

                        decimal gross = baseSalary * (1m + bonusRate / 100m);
                        decimal net   = gross * (1m - office.Country.TaxBase / 100m);

                        MonthlyPayment payment = new MonthlyPayment
                        {
                            Employee     = employee,
                            Year         = year,
                            Month        = month,
                            SalaryGross  = Math.Round(gross, 2, MidpointRounding.AwayFromZero),
                            SalaryNet    = Math.Round(net, 2, MidpointRounding.AwayFromZero),
                            BonusApplied = Math.Round(bonusRate, 2, MidpointRounding.AwayFromZero)
                        };

                        result.GeneratedPayments.Add(payment);
                    }
                }

                // 9. Todas las nóminas en un único INSERT: se guardan todas o ninguna
                MonthlyPayment.InsertAll(result.GeneratedPayments);
                result.Success                = true;
                result.ProcessedEmployeesCount =
                    result.GeneratedPayments.Select(p => p.Employee.IdEmployee).Distinct().Count();
                result.Message =
                    $"Se han generado {result.GeneratedPayments.Count} nómina(s) del periodo {year}/{month:D2} " +
                    $"para {result.ProcessedEmployeesCount} empleado(s).";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message =
                    $"Error al generar nóminas del periodo {year}/{month:D2}. " +
                    $"No se ha guardado ninguna nómina. Detalle: {ex.Message}";
                result.GeneratedPayments.Clear();
            }

            return result;
        }

        /// <summary>
        /// Días que el puesto estuvo activo entre el primer y el último día del mes (0 si ninguno).
        /// </summary>
        private static int CalculateDaysWorkedInMonth(
            Position position,
            DateTime firstDay,
            DateTime lastDay)
        {
            DateTime effectiveStart = position.StartDate > firstDay
                ? position.StartDate
                : firstDay;

            DateTime effectiveEnd =
                (!position.EndDate.HasValue || position.EndDate.Value > lastDay)
                ? lastDay
                : position.EndDate.Value;

            int days = (effectiveEnd - effectiveStart).Days + 1;
            return days > 0 ? days : 0;
        }
    }
}
