using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.RowReader;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: MonthlyPayment (Nómina Mensual / Pago Mensual).
    /// Mapea la tabla 'MonthlyPayments' del esquema relacional.
    /// Almacena el resultado del cálculo y liquidación de salarios para un empleado en un año y mes:
    /// - Salario bruto devengado (proporcional con bonus de trienios aplicado).
    /// - Salario neto resultante tras la retención impositiva del país de la oficina.
    /// - Porcentaje de bonus por trienios devengado.
    /// Aplica estrictamente los principios de POO: la clave foránea payEmployee se modela
    /// como una referencia directa al objeto de dominio Employee, no como un tipo primitivo.
    /// </summary>
    public class MonthlyPayment
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Identificador único autonumérico de la nómina (Clave Primaria, INT).
        /// </summary>
        public int IdMonthlyPayment { get; set; }

        /// <summary>
        /// Relación de Dominio POO: Empleado titular de la nómina.
        /// Corresponde a la clave foránea 'payEmployee' vinculada a la tabla Employees.
        /// </summary>
        public Employee Employee { get; set; } = new Employee();

        /// <summary>
        /// Año natural de devengo de la nómina (INT, ej. 2026).
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Mes natural de devengo de la nómina (INT, 1 a 12).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Salario mensual bruto calculado (DECIMAL(11,2)).
        /// Incluye la proporción de días trabajados en cada puesto y el bonus por antigüedad.
        /// </summary>
        public decimal SalaryGross { get; set; }

        /// <summary>
        /// Salario mensual neto resultante (DECIMAL(11,2)).
        /// Salario bruto menos la retención fiscal aplicable en el país de la oficina.
        /// </summary>
        public decimal SalaryNet { get; set; }

        /// <summary>
        /// Porcentaje de bonus por trienios aplicado en la nómina (DECIMAL(4,2)).
        /// </summary>
        public decimal BonusApplied { get; set; }

        /// <summary>
        /// Ciudad de la oficina a la que corresponde la nómina. No es una columna de la tabla:
        /// se deduce de los puestos del empleado (ver MonthlyPaymentDAO.PaymentOfficeSql).
        /// </summary>
        public string OfficeName { get; set; } = string.Empty;

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para enlaces de datos, serialización y mapeo.
        /// </summary>
        public MonthlyPayment()
        {
        }

        /// <summary>
        /// Constructor parametrizado para nuevas nóminas (sin identificador de base de datos).
        /// </summary>
        /// <param name="employee">Objeto Employee titular del pago.</param>
        /// <param name="year">Año de liquidación.</param>
        /// <param name="month">Mes de liquidación (1 a 12).</param>
        /// <param name="salaryGross">Salario bruto liquidado.</param>
        /// <param name="salaryNet">Salario neto a percibir.</param>
        /// <param name="bonusApplied">Porcentaje de bonus aplicado.</param>
        public MonthlyPayment(Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee), "El empleado titular no puede ser nulo.");
            Year = year;
            Month = month;
            SalaryGross = salaryGross;
            SalaryNet = salaryNet;
            BonusApplied = bonusApplied;
        }

        /// <summary>
        /// Constructor parametrizado completo (para reconstrucción desde la persistencia).
        /// </summary>
        /// <param name="idMonthlyPayment">Identificador único de la nómina.</param>
        /// <param name="employee">Objeto Employee titular del pago.</param>
        /// <param name="year">Año de liquidación.</param>
        /// <param name="month">Mes de liquidación (1 a 12).</param>
        /// <param name="salaryGross">Salario bruto liquidado.</param>
        /// <param name="salaryNet">Salario neto a percibir.</param>
        /// <param name="bonusApplied">Porcentaje de bonus aplicado.</param>
        public MonthlyPayment(int idMonthlyPayment, Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
            : this(employee, year, month, salaryGross, salaryNet, bonusApplied)
        {
            IdMonthlyPayment = idMonthlyPayment;
        }

        #endregion

        #region Métodos de Persistencia (delegan en MonthlyPaymentDAO)

        /// <summary>
        /// DAO con el SQL de la tabla MonthlyPayments. La entidad no accede a la base de datos directamente.
        /// </summary>
        private static readonly MonthlyPaymentDAO dao = new MonthlyPaymentDAO();

        /// <summary>
        /// Inserta la nómina.
        /// </summary>
        public bool Insert()
        {
            Validate();
            return dao.Insert(Employee.IdEmployee, Year, Month, SalaryGross, SalaryNet, BonusApplied) > 0;
        }

        /// <summary>
        /// Inserta varias nóminas en una única sentencia: o se guardan todas o ninguna.
        /// El orden de la lista se conserva en los identificadores generados.
        /// </summary>
        public static int InsertAll(IReadOnlyCollection<MonthlyPayment> payments)
        {
            var values = new List<(int, int, int, decimal, decimal, decimal)>();
            foreach (MonthlyPayment p in payments)
            {
                p.Validate();
                values.Add((p.Employee.IdEmployee, p.Year, p.Month, p.SalaryGross, p.SalaryNet, p.BonusApplied));
            }

            return dao.InsertAll(values);
        }

        /// <summary>
        /// Actualiza una nómina existente.
        /// </summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdMonthlyPayment, Employee.IdEmployee, Year, Month, SalaryGross, SalaryNet, BonusApplied) > 0;
        }

        /// <summary>
        /// Elimina la nómina.
        /// </summary>
        public bool Delete()
        {
            return dao.Delete(IdMonthlyPayment) > 0;
        }

        /// <summary>
        /// Lee todas las nóminas registradas.
        /// </summary>
        public static List<MonthlyPayment> ReadAll()
        {
            return FromRows(dao.ReadAll());
        }

        /// <summary>
        /// Obtiene todas las nóminas de un año y mes, opcionalmente filtradas por oficina.
        /// Ordenadas de mayor sueldo bruto a menor.
        /// </summary>
        public static List<MonthlyPayment> ReadByPeriod(int year, int month, int? officeId = null)
        {
            return FromRows(dao.ReadByPeriod(year, month, officeId));
        }

        /// <summary>
        /// Obtiene todas las nóminas de un empleado, de más reciente a más antigua.
        /// </summary>
        public static List<MonthlyPayment> ReadByEmployee(int employeeId)
        {
            return FromRows(dao.ReadByEmployee(employeeId));
        }

        /// <summary>
        /// Comprueba si ya se han generado previamente nóminas para el mes y año dados.
        /// </summary>
        public static bool PeriodAlreadyGenerated(int year, int month)
        {
            return dao.CountByPeriod(year, month) > 0;
        }

        private static List<MonthlyPayment> FromRows(List<string[]> rows)
        {
            var list = new List<MonthlyPayment>();
            foreach (string[] row in rows)
            {
                list.Add(FromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Crea el objeto a partir de una fila del DAO, incluido su objeto Employee.
        /// </summary>
        private static MonthlyPayment FromRow(string[] row)
        {
            var employee = new Employee
            {
                IdEmployee = ReadInt(row[1]),
                Name = row[7],
                Country = new Country { IdCountry = row[8], Name = row[10], TaxBase = ReadDecimal(row[11]) },
                IBAN = row[9]
            };

            return new MonthlyPayment
            {
                IdMonthlyPayment = ReadInt(row[0]),
                Employee = employee,
                Year = ReadInt(row[2]),
                Month = ReadInt(row[3]),
                SalaryGross = ReadDecimal(row[4]),
                SalaryNet = ReadDecimal(row[5]),
                BonusApplied = ReadDecimal(row[6]),
                OfficeName = row[12]
            };
        }

        #endregion

        #region Reglas de Validación de Negocio

        /// <summary>
        /// Valida la consistencia de los datos de la nómina.
        /// </summary>
        public void Validate()
        {
            if (Employee == null || Employee.IdEmployee <= 0)
            {
                throw new ArgumentException("La nómina debe estar asignada a un empleado válido.");
            }

            if (Year < 1900 || Year > 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(Year), "El año especificado no es válido.");
            }

            if (Month < 1 || Month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(Month), "El mes debe estar comprendido entre 1 y 12.");
            }

            if (SalaryGross < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SalaryGross), "El salario bruto no puede ser negativo.");
            }

            if (SalaryNet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SalaryNet), "El salario neto no puede ser negativo.");
            }

            if (BonusApplied < 0 || BonusApplied > 99.99m)
            {
                throw new ArgumentOutOfRangeException(nameof(BonusApplied), "El bonus aplicado debe estar entre 0.00% y 99.99%.");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"Nómina {Year}-{Month:D2}: {Employee.Name} - Bruto: {SalaryGross:C2} | Neto: {SalaryNet:C2}";
        }
    }
}
