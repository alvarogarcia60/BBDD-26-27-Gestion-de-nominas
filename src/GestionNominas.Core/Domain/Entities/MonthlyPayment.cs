using System;
using System.Collections.Generic;
using Company.persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Nómina mensual de un empleado (tabla MonthlyPayments).
    /// El empleado (payEmployee) se guarda como un objeto Employee.
    /// </summary>
    public class MonthlyPayment
    {
        #region Propiedades

        /// <summary>Clave primaria (autonumérica).</summary>
        public int IdMonthlyPayment { get; set; }

        /// <summary>Empleado de la nómina (clave foránea payEmployee).</summary>
        public Employee Employee { get; set; } = new Employee();

        /// <summary>Año de la nómina.</summary>
        public int Year { get; set; }

        /// <summary>Mes de la nómina (1 a 12).</summary>
        public int Month { get; set; }

        /// <summary>Salario bruto del mes, con el bonus por trienios.</summary>
        public decimal SalaryGross { get; set; }

        /// <summary>Salario neto: bruto menos el impuesto del país de la oficina.</summary>
        public decimal SalaryNet { get; set; }

        /// <summary>Porcentaje de bonus por trienios aplicado.</summary>
        public decimal BonusApplied { get; set; }

        /// <summary>
        /// Ciudad de la oficina de la nómina. No es una columna de la tabla: se deduce de los
        /// puestos del empleado (ver MonthlyPaymentDAO.PaymentOfficeSql).
        /// </summary>
        public string OfficeName { get; set; } = string.Empty;

        #endregion

        #region Constructores

        public MonthlyPayment()
        {
        }

        /// <summary>Nómina nueva, todavía sin identificador.</summary>
        public MonthlyPayment(Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee), "El empleado titular no puede ser nulo.");
            Year = year;
            Month = month;
            SalaryGross = salaryGross;
            SalaryNet = salaryNet;
            BonusApplied = bonusApplied;
        }

        /// <summary>Nómina ya guardada, con su identificador.</summary>
        public MonthlyPayment(int idMonthlyPayment, Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
            : this(employee, year, month, salaryGross, salaryNet, bonusApplied)
        {
            IdMonthlyPayment = idMonthlyPayment;
        }

        #endregion

        #region Persistencia (a través de MonthlyPaymentDAO)

        /// <summary>DAO de las nóminas: la entidad no accede directamente a la base de datos.</summary>
        private static readonly MonthlyPaymentDAO dao = new MonthlyPaymentDAO();

        /// <summary>
        /// Valida y guarda varias nóminas en una única sentencia: se guardan todas o ninguna.
        /// Devuelve el número de nóminas guardadas.
        /// </summary>
        public static int InsertAll(IReadOnlyCollection<MonthlyPayment> payments)
        {
            foreach (MonthlyPayment payment in payments)
            {
                payment.Validate();
            }

            return dao.InsertAll(payments);
        }

        /// <summary>Nóminas de un mes (opcionalmente de una oficina), de mayor a menor bruto.</summary>
        public static List<MonthlyPayment> ReadByPeriod(int year, int month, int? officeId = null)
        {
            dao.ReadByPeriod(year, month, officeId);
            return dao.Payments;
        }

        /// <summary>Nóminas de un empleado, de la más reciente a la más antigua.</summary>
        public static List<MonthlyPayment> ReadByEmployee(int employeeId)
        {
            dao.ReadByEmployee(employeeId);
            return dao.Payments;
        }

        /// <summary>Indica si ya se han generado las nóminas de ese mes.</summary>
        public static bool PeriodAlreadyGenerated(int year, int month)
        {
            dao.CountByPeriod(year, month);
            return dao.Total > 0;
        }

        #endregion

        #region Validación

        /// <summary>Lanza una excepción si algún dato no es válido.</summary>
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
