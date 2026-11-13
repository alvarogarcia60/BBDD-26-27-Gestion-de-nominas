using System;
using System.Data;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Consultas de los informes 3, 4 y 5: las ejecuta con AgenteBD y deja el resultado en Table.
    /// La oficina de cada nómina se obtiene con MonthlyPaymentDAO.PaymentOfficeSql.
    /// </summary>
    internal class ReportDAO
    {
        /// <summary>Tabla del último informe leído.</summary>
        public DataTable Table { get; private set; } = new DataTable();

        /// <summary>
        /// Informe 3: empleados de cada sede (oficina del puesto actual) con los días trabajados,
        /// trienios y bonus acumulados hasta hoy en cualquier puesto.
        /// </summary>
        public void EmployeesWithTrieniosByOffice()
        {
            Load(@"
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
                ORDER BY o.offCity, e.empName;",
                ("ID Sede", typeof(int)), ("Sede", typeof(string)), ("ID Empleado", typeof(int)),
                ("Empleado", typeof(string)), ("Puesto Actual", typeof(string)), ("Días Trabajados", typeof(int)),
                ("Trienios", typeof(int)), ("Bonus Trienios (%)", typeof(decimal)));
        }

        /// <summary>
        /// Informe 4: gasto en nóminas de cada oficina en un año-mes.
        /// Si un empleado cambió de oficina, cada una de sus nóminas cuenta en su oficina.
        /// </summary>
        public void OfficeExpensesByMonth(int year, int month)
        {
            Load($@"
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
                ORDER BY SUM(m.paySalaryGross) DESC;",
                ("ID Oficina", typeof(int)), ("Ciudad", typeof(string)), ("País", typeof(string)), ("Empleados", typeof(int)),
                ("Gasto Bruto Total (€)", typeof(decimal)), ("Gasto Neto Total (€)", typeof(decimal)));
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada año-mes con datos.
        /// </summary>
        public void CountryExpensesByMonth()
        {
            Load($@"
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
                ORDER BY c.couName, m.payYear DESC, m.payMonth DESC;",
                ("Código País", typeof(string)), ("País", typeof(string)), ("Año", typeof(int)), ("Mes", typeof(int)),
                ("Empleados", typeof(int)), ("Gasto Bruto (€)", typeof(decimal)), ("Gasto Neto (€)", typeof(decimal)),
                ("Bonus Medio (%)", typeof(decimal)));
        }

        /// <summary>
        /// Ejecuta la consulta y construye la tabla con las columnas indicadas
        /// (AgenteBD devuelve solo valores, sin nombres ni tipos de columna).
        /// </summary>
        private void Load(string sql, params (string Name, Type Type)[] columns)
        {
            Table = new DataTable();
            foreach (var column in columns)
            {
                Table.Columns.Add(column.Name, column.Type);
            }

            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                DataRow dataRow = Table.NewRow();
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i].Type == typeof(int)) dataRow[i] = ReadInt(row[i]);
                    else if (columns[i].Type == typeof(decimal)) dataRow[i] = ReadDecimal(row[i]);
                    else dataRow[i] = row[i];
                }
                Table.Rows.Add(dataRow);
            }
        }
    }
}
