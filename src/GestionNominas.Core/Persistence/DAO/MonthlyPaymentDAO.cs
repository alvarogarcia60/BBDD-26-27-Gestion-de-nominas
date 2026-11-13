using System.Collections.Generic;
using GestionNominas.Domain.Entities;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Acceso a la tabla MonthlyPayments: ejecuta el SQL con AgenteBD y construye los objetos MonthlyPayment.
    /// Las lecturas dejan el resultado en Payments o en Total.
    /// </summary>
    internal class MonthlyPaymentDAO
    {
        /// <summary>
        /// Subconsulta con la oficina de cada nómina (idMonthlyPayment, posOffice).
        /// La tabla MonthlyPayments no guarda la oficina: como las nóminas de un empleado en un mes
        /// se generan por orden de llegada a cada oficina, la 1.ª nómina del mes corresponde a la
        /// 1.ª oficina en la que trabajó ese mes, la 2.ª a la 2.ª, etc.
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

        private const string SelectSql = @"
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

        private const string InsertSql =
            "INSERT INTO MonthlyPayments (payEmployee, payYear, payMonth, paySalaryGross, paySalaryNet, payBonusApplied) VALUES";

        /// <summary>Nóminas construidas en la última lectura.</summary>
        public List<MonthlyPayment> Payments { get; private set; } = new List<MonthlyPayment>();

        /// <summary>Resultado del último recuento.</summary>
        public int Total { get; private set; }

        /// <summary>Lee las nóminas de un año y mes (opcionalmente de una oficina), de mayor a menor bruto.</summary>
        public void ReadByPeriod(int year, int month, int? idOffice)
        {
            string officeFilter = idOffice.HasValue && idOffice.Value > 0 ? $"AND po.posOffice = {idOffice.Value}" : string.Empty;

            Load($@"{SelectSql}
                WHERE m.payYear = {year} AND m.payMonth = {month} {officeFilter}
                ORDER BY m.paySalaryGross DESC;");
        }

        /// <summary>Lee las nóminas de un empleado, de la más reciente a la más antigua.</summary>
        public void ReadByEmployee(int idEmployee)
        {
            Load($@"{SelectSql}
                WHERE m.payEmployee = {idEmployee}
                ORDER BY m.payYear DESC, m.payMonth DESC, m.idMonthlyPayment ASC;");
        }

        /// <summary>
        /// Inserta varias nóminas en una única sentencia, así se guardan todas o ninguna.
        /// Devuelve las filas afectadas.
        /// </summary>
        public int InsertAll(IReadOnlyCollection<MonthlyPayment> payments)
        {
            if (payments.Count == 0) return 0;

            var values = new List<string>();
            foreach (MonthlyPayment p in payments)
            {
                values.Add(Values(p.Employee.IdEmployee, p.Year, p.Month, p.SalaryGross, p.SalaryNet, p.BonusApplied));
            }

            return AgenteBD.GetInstance().Change($"{InsertSql} {string.Join(", ", values)};");
        }

        /// <summary>Cuenta las nóminas ya generadas para el año y mes.</summary>
        public void CountByPeriod(int year, int month)
        {
            Total = ReadInt(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM MonthlyPayments WHERE payYear = {year} AND payMonth = {month};")[0][0]);
        }

        private static string Values(int idEmployee, int year, int month, decimal gross, decimal net, decimal bonus) =>
            $"({idEmployee}, {year}, {month}, {SqlNumber(gross)}, {SqlNumber(net)}, {SqlNumber(bonus)})";

        /// <summary>Ejecuta el SELECT y convierte cada fila en un MonthlyPayment con su Employee.</summary>
        private void Load(string sql)
        {
            Payments = new List<MonthlyPayment>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                var employee = new Employee
                {
                    IdEmployee = ReadInt(row[1]),
                    Name = row[7],
                    Country = new Country { IdCountry = row[8], Name = row[10], TaxBase = ReadDecimal(row[11]) },
                    IBAN = row[9]
                };

                Payments.Add(new MonthlyPayment
                {
                    IdMonthlyPayment = ReadInt(row[0]),
                    Employee = employee,
                    Year = ReadInt(row[2]),
                    Month = ReadInt(row[3]),
                    SalaryGross = ReadDecimal(row[4]),
                    SalaryNet = ReadDecimal(row[5]),
                    BonusApplied = ReadDecimal(row[6]),
                    OfficeName = row[12]
                });
            }
        }
    }
}
