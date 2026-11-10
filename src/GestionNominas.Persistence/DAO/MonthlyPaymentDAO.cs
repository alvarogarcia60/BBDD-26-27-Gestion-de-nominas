using System.Collections.Generic;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// DAO de la tabla MonthlyPayments. Contiene todo el SQL de las nóminas y lo ejecuta con AgenteBD.
    /// Cada fila se devuelve con las columnas:
    ///  0 idMonthlyPayment, 1 payEmployee, 2 payYear, 3 payMonth, 4 paySalaryGross, 5 paySalaryNet,
    ///  6 payBonusApplied, 7 empName, 8 empCountry, 9 empIBAN, 10 couName, 11 couTaxBase (país del empleado),
    ///  12 offCity (ciudad de la oficina de la nómina).
    /// </summary>
    internal class MonthlyPaymentDAO
    {
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

        public List<string[]> ReadAll()
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} ORDER BY m.payYear DESC, m.payMonth DESC, m.paySalaryGross DESC;");
        }

        /// <summary>Nóminas de un año y mes (opcionalmente de una oficina), de mayor a menor bruto.</summary>
        public List<string[]> ReadByPeriod(int year, int month, int? idOffice)
        {
            string officeFilter = idOffice.HasValue && idOffice.Value > 0 ? $"AND po.posOffice = {idOffice.Value}" : string.Empty;

            return AgenteBD.GetInstance().Read($@"{SelectSql}
                WHERE m.payYear = {year} AND m.payMonth = {month} {officeFilter}
                ORDER BY m.paySalaryGross DESC;");
        }

        /// <summary>Nóminas de un empleado, de la más reciente a la más antigua.</summary>
        public List<string[]> ReadByEmployee(int idEmployee)
        {
            return AgenteBD.GetInstance().Read($@"{SelectSql}
                WHERE m.payEmployee = {idEmployee}
                ORDER BY m.payYear DESC, m.payMonth DESC, m.idMonthlyPayment ASC;");
        }

        public int Insert(int idEmployee, int year, int month, decimal gross, decimal net, decimal bonus)
        {
            return AgenteBD.GetInstance().Change($"{InsertSql} {Values(idEmployee, year, month, gross, net, bonus)};");
        }

        /// <summary>
        /// Inserta varias nóminas en una única sentencia: o se guardan todas o ninguna.
        /// El orden de la lista se conserva en los identificadores generados.
        /// </summary>
        public int InsertAll(List<(int IdEmployee, int Year, int Month, decimal Gross, decimal Net, decimal Bonus)> payments)
        {
            if (payments.Count == 0) return 0;

            var values = new List<string>();
            foreach (var p in payments)
            {
                values.Add(Values(p.IdEmployee, p.Year, p.Month, p.Gross, p.Net, p.Bonus));
            }

            return AgenteBD.GetInstance().Change($"{InsertSql} {string.Join(", ", values)};");
        }

        public int Update(int idMonthlyPayment, int idEmployee, int year, int month, decimal gross, decimal net, decimal bonus)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE MonthlyPayments SET payEmployee = {idEmployee}, payYear = {year}, payMonth = {month}, " +
                $"paySalaryGross = {SqlNumber(gross)}, paySalaryNet = {SqlNumber(net)}, " +
                $"payBonusApplied = {SqlNumber(bonus)} WHERE idMonthlyPayment = {idMonthlyPayment};");
        }

        public int Delete(int idMonthlyPayment)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM MonthlyPayments WHERE idMonthlyPayment = {idMonthlyPayment};");
        }

        /// <summary>Número de nóminas ya generadas para el año y mes.</summary>
        public int CountByPeriod(int year, int month)
        {
            return ReadCount(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM MonthlyPayments WHERE payYear = {year} AND payMonth = {month};")[0][0]);
        }

        private static string Values(int idEmployee, int year, int month, decimal gross, decimal net, decimal bonus) =>
            $"({idEmployee}, {year}, {month}, {SqlNumber(gross)}, {SqlNumber(net)}, {SqlNumber(bonus)})";
    }
}
