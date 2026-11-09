using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

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
        /// se deduce de los puestos del empleado (ver <see cref="PaymentOfficeSql"/>).
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

        #region Métodos de Persistencia (AgenteBD)

        /// <summary>
        /// Inserta la nómina.
        /// </summary>
        public bool Insert()
        {
            Validate();
            return AgenteBD.GetInstance().Change($"{InsertSql} {SqlValues()};") > 0;
        }

        /// <summary>
        /// Inserta varias nóminas en una única sentencia: o se guardan todas o ninguna.
        /// El orden de la lista se conserva en los identificadores generados.
        /// </summary>
        public static int InsertAll(IReadOnlyCollection<MonthlyPayment> payments)
        {
            if (payments.Count == 0) return 0;

            var values = new List<string>();
            foreach (MonthlyPayment payment in payments)
            {
                payment.Validate();
                values.Add(payment.SqlValues());
            }

            return AgenteBD.GetInstance().Change($"{InsertSql} {string.Join(", ", values)};");
        }

        private const string InsertSql =
            "INSERT INTO MonthlyPayments (payEmployee, payYear, payMonth, paySalaryGross, paySalaryNet, payBonusApplied) VALUES";

        private string SqlValues() =>
            $"({Employee.IdEmployee}, {Year}, {Month}, {SqlNumber(SalaryGross)}, {SqlNumber(SalaryNet)}, {SqlNumber(BonusApplied)})";

        /// <summary>
        /// Actualiza una nómina existente.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = $"UPDATE MonthlyPayments SET payEmployee = {Employee.IdEmployee}, payYear = {Year}, payMonth = {Month}, " +
                         $"paySalaryGross = {SqlNumber(SalaryGross)}, paySalaryNet = {SqlNumber(SalaryNet)}, " +
                         $"payBonusApplied = {SqlNumber(BonusApplied)} WHERE idMonthlyPayment = {IdMonthlyPayment};";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Elimina la nómina.
        /// </summary>
        public bool Delete()
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM MonthlyPayments WHERE idMonthlyPayment = {IdMonthlyPayment};") > 0;
        }

        /// <summary>
        /// Lee todas las nóminas registradas.
        /// </summary>
        public static List<MonthlyPayment> ReadAll()
        {
            return ReadList($"{SelectWithOfficeSql} ORDER BY m.payYear DESC, m.payMonth DESC, m.paySalaryGross DESC;");
        }

        /// <summary>
        /// Obtiene todas las nóminas de un año y mes, opcionalmente filtradas por oficina.
        /// Ordenadas de mayor sueldo bruto a menor.
        /// </summary>
        public static List<MonthlyPayment> ReadByPeriod(int year, int month, int? officeId = null)
        {
            string officeFilter = officeId.HasValue && officeId.Value > 0 ? $"AND po.posOffice = {officeId.Value}" : string.Empty;

            return ReadList($@"{SelectWithOfficeSql}
                               WHERE m.payYear = {year} AND m.payMonth = {month} {officeFilter}
                               ORDER BY m.paySalaryGross DESC;");
        }

        /// <summary>
        /// Obtiene todas las nóminas de un empleado, de más reciente a más antigua.
        /// </summary>
        public static List<MonthlyPayment> ReadByEmployee(int employeeId)
        {
            return ReadList($@"{SelectWithOfficeSql}
                               WHERE m.payEmployee = {employeeId}
                               ORDER BY m.payYear DESC, m.payMonth DESC, m.idMonthlyPayment ASC;");
        }

        private static List<MonthlyPayment> ReadList(string sql)
        {
            var list = new List<MonthlyPayment>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Subconsulta SQL que devuelve la oficina de cada nómina (idMonthlyPayment, posOffice).
        /// El esquema oficial no guarda la oficina en MonthlyPayments, así que se deduce: un empleado
        /// tiene una nómina por oficina en el mes, generadas por orden de llegada a cada oficina,
        /// de modo que la N-ésima nómina del mes corresponde a la N-ésima oficina en la que trabajó.
        /// Requiere que los puestos de ese mes no se modifiquen después de generar las nóminas.
        /// </summary>
        internal const string PaymentOfficeSql = @"
            SELECT pay.idMonthlyPayment, oft.posOffice
            FROM (
                SELECT idMonthlyPayment, payEmployee, payYear, payMonth,
                       ROW_NUMBER() OVER (PARTITION BY payEmployee, payYear, payMonth
                                          ORDER BY idMonthlyPayment) AS seq
                FROM MonthlyPayments
            ) AS pay
            INNER JOIN (
                SELECT worked.posEmployee, worked.posOffice, worked.payYear, worked.payMonth,
                       ROW_NUMBER() OVER (PARTITION BY worked.posEmployee, worked.payYear, worked.payMonth
                                          ORDER BY worked.firstStart) AS seq
                FROM (
                    SELECT p.posEmployee, p.posOffice, meses.payYear, meses.payMonth,
                           MIN(p.posStartDate) AS firstStart
                    FROM Positions p
                    INNER JOIN (SELECT DISTINCT payYear, payMonth FROM MonthlyPayments) AS meses
                        ON  p.posStartDate <= LAST_DAY(MAKEDATE(meses.payYear, 1) + INTERVAL (meses.payMonth - 1) MONTH)
                        AND (p.posEndDate IS NULL
                             OR p.posEndDate >= MAKEDATE(meses.payYear, 1) + INTERVAL (meses.payMonth - 1) MONTH)
                    GROUP BY p.posEmployee, p.posOffice, meses.payYear, meses.payMonth
                ) AS worked
            ) AS oft
                ON  oft.posEmployee = pay.payEmployee
                AND oft.payYear     = pay.payYear
                AND oft.payMonth    = pay.payMonth
                AND oft.seq         = pay.seq";

        private const string SelectWithOfficeSql = @"
            SELECT m.idMonthlyPayment, m.payEmployee, m.payYear, m.payMonth,
                   m.paySalaryGross, m.paySalaryNet, m.payBonusApplied,
                   e.empName, e.empCountry, e.empIBAN,
                   c.couName, c.couTaxBase,
                   o.offCity
            FROM MonthlyPayments m
            INNER JOIN Employees e ON m.payEmployee = e.idEmployee
            INNER JOIN Countries c ON e.empCountry = c.idCountry
            LEFT JOIN (" + PaymentOfficeSql + @") AS po ON po.idMonthlyPayment = m.idMonthlyPayment
            LEFT JOIN Offices o ON o.idOffice = po.posOffice";

        /// <summary>
        /// Comprueba si ya se han generado previamente nóminas para el mes y año dados.
        /// </summary>
        public static bool PeriodAlreadyGenerated(int year, int month)
        {
            string sql = $"SELECT COUNT(*) FROM MonthlyPayments WHERE payYear = {year} AND payMonth = {month};";
            return ReadInt(AgenteBD.GetInstance().Read(sql)[0][0]) > 0;
        }

        private static MonthlyPayment MapFromRow(string[] row)
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
