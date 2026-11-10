using System.Collections.Generic;

namespace Company.persistence
{
    /// <summary>
    /// DAO de los informes 3, 4 y 5: consultas agregadas sobre varias tablas.
    /// Cada nómina se atribuye a su oficina con <see cref="MonthlyPaymentDAO.PaymentOfficeSql"/>.
    /// </summary>
    internal class ReportDAO
    {
        /// <summary>
        /// Informe 3: empleados de cada sede (oficina del puesto actual) con los días trabajados,
        /// trienios y bonus acumulados hasta hoy en cualquier puesto.
        /// </summary>
        public List<string[]> EmployeesWithTrieniosByOffice()
        {
            return AgenteBD.GetInstance().Read(@"
                SELECT
                    o.idOffice,
                    CONCAT(o.offCity, ' (', o.offCountry, ')'),
                    e.idEmployee,
                    e.empName,
                    p_curr.posName,
                    hist.historicalDays,
                    FLOOR(hist.historicalDays / 1095),
                    ROUND(FLOOR(hist.historicalDays / 1095) * o.offThreeYearBonus, 2)
                FROM Employees e
                /* Puesto actual: el puesto sin fecha de fin ya iniciado */
                INNER JOIN Positions p_curr
                    ON  p_curr.posEmployee = e.idEmployee
                    AND p_curr.posEndDate IS NULL
                    AND p_curr.posStartDate <= CURDATE()
                INNER JOIN Offices o ON p_curr.posOffice = o.idOffice
                /* Días trabajados hasta hoy en todos los puestos (se ignoran días futuros) */
                INNER JOIN (
                    SELECT posEmployee,
                           SUM(DATEDIFF(LEAST(COALESCE(posEndDate, CURDATE()), CURDATE()), posStartDate) + 1) AS historicalDays
                    FROM Positions
                    WHERE posStartDate <= CURDATE()
                    GROUP BY posEmployee
                ) AS hist ON hist.posEmployee = e.idEmployee
                ORDER BY o.offCity, e.empName;");
        }

        /// <summary>
        /// Informe 4: gasto en nóminas de cada oficina en un año-mes.
        /// Si un empleado cambió de oficina, cada una de sus nóminas cuenta en su oficina.
        /// </summary>
        public List<string[]> OfficeExpensesByMonth(int year, int month)
        {
            return AgenteBD.GetInstance().Read($@"
                SELECT
                    o.idOffice,
                    o.offCity,
                    c.couName,
                    COUNT(*),
                    ROUND(SUM(m.paySalaryGross), 2),
                    ROUND(SUM(m.paySalaryNet), 2)
                FROM MonthlyPayments m
                INNER JOIN ({MonthlyPaymentDAO.PaymentOfficeSql}) po ON po.idMonthlyPayment = m.idMonthlyPayment
                INNER JOIN Offices   o ON po.posOffice  = o.idOffice
                INNER JOIN Countries c ON o.offCountry  = c.idCountry
                WHERE m.payYear = {year} AND m.payMonth = {month}
                GROUP BY o.idOffice, o.offCity, c.couName
                ORDER BY SUM(m.paySalaryGross) DESC;");
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada año-mes con datos.
        /// </summary>
        public List<string[]> CountryExpensesByMonth()
        {
            return AgenteBD.GetInstance().Read($@"
                SELECT
                    c.idCountry,
                    c.couName,
                    m.payYear,
                    m.payMonth,
                    COUNT(DISTINCT m.payEmployee),
                    ROUND(SUM(m.paySalaryGross), 2),
                    ROUND(SUM(m.paySalaryNet), 2),
                    ROUND(AVG(m.payBonusApplied), 2)
                FROM MonthlyPayments m
                INNER JOIN ({MonthlyPaymentDAO.PaymentOfficeSql}) po ON po.idMonthlyPayment = m.idMonthlyPayment
                INNER JOIN Offices   o ON po.posOffice     = o.idOffice
                INNER JOIN Countries c ON o.offCountry     = c.idCountry
                GROUP BY c.idCountry, c.couName, m.payYear, m.payMonth
                ORDER BY c.couName, m.payYear DESC, m.payMonth DESC;");
        }
    }
}
