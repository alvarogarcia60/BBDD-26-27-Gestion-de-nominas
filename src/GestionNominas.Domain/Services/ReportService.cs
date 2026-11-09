using System;
using System.Collections.Generic;
using System.Data;
using GestionNominas.Domain.Entities;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio: Informes agregados de nóminas.
    /// Centraliza las consultas de los informes para que la capa de presentación
    /// no acceda directamente a la capa de persistencia (arquitectura en 3 capas).
    /// Cada nómina se atribuye a su oficina (ver <see cref="MonthlyPayment.PaymentOfficeSql"/>).
    /// </summary>
    public static class ReportService
    {
        /// <summary>
        /// Informe 3: empleados de cada sede (oficina del puesto actual) con los trienios
        /// completos acumulados hasta hoy en cualquier puesto.
        /// </summary>
        public static DataTable GetEmployeesWithTrieniosByOffice()
        {
            string sql = @"
                SELECT
                    o.idOffice                                                         AS 'ID Sede',
                    CONCAT(o.offCity, ' (', o.offCountry, ')')                         AS 'Sede',
                    e.idEmployee                                                       AS 'ID Empleado',
                    e.empName                                                          AS 'Empleado',
                    p_curr.posName                                                     AS 'Puesto Actual',
                    hist.historicalDays                                                AS 'Días Trabajados',
                    FLOOR(hist.historicalDays / 1095)                                  AS 'Trienios',
                    ROUND(FLOOR(hist.historicalDays / 1095) * o.offThreeYearBonus, 2)  AS 'Bonus Trienios (%)'
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
                ORDER BY o.offCity, e.empName;";

            return ToTable(sql,
                ("ID Sede", typeof(int)), ("Sede", typeof(string)), ("ID Empleado", typeof(int)),
                ("Empleado", typeof(string)), ("Puesto Actual", typeof(string)), ("Días Trabajados", typeof(int)),
                ("Trienios", typeof(int)), ("Bonus Trienios (%)", typeof(decimal)));
        }

        /// <summary>
        /// Informe 4: gasto en nóminas de cada oficina en un año-mes.
        /// Si un empleado cambió de oficina, cada una de sus nóminas cuenta en su oficina.
        /// </summary>
        public static DataTable GetOfficeExpensesByMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "El mes debe estar comprendido entre 1 y 12.");

            string sql = $@"
                SELECT
                    o.idOffice                       AS 'ID Oficina',
                    o.offCity                        AS 'Ciudad',
                    c.couName                        AS 'País',
                    COUNT(*)                         AS 'Empleados',
                    ROUND(SUM(m.paySalaryGross), 2)  AS 'Gasto Bruto Total (€)',
                    ROUND(SUM(m.paySalaryNet), 2)    AS 'Gasto Neto Total (€)'
                FROM MonthlyPayments m
                INNER JOIN ({MonthlyPayment.PaymentOfficeSql}) po ON po.idMonthlyPayment = m.idMonthlyPayment
                INNER JOIN Offices   o ON po.posOffice  = o.idOffice
                INNER JOIN Countries c ON o.offCountry  = c.idCountry
                WHERE m.payYear = {year} AND m.payMonth = {month}
                GROUP BY o.idOffice, o.offCity, c.couName
                ORDER BY SUM(m.paySalaryGross) DESC;";

            return ToTable(sql,
                ("ID Oficina", typeof(int)), ("Ciudad", typeof(string)), ("País", typeof(string)), ("Empleados", typeof(int)),
                ("Gasto Bruto Total (€)", typeof(decimal)), ("Gasto Neto Total (€)", typeof(decimal)));
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada año-mes con datos.
        /// </summary>
        public static DataTable GetCountryExpensesByMonth()
        {
            string sql = $@"
                SELECT
                    c.idCountry                      AS 'Código País',
                    c.couName                        AS 'País',
                    m.payYear                        AS 'Año',
                    m.payMonth                       AS 'Mes',
                    COUNT(DISTINCT m.payEmployee)    AS 'Empleados',
                    ROUND(SUM(m.paySalaryGross), 2)  AS 'Gasto Bruto (€)',
                    ROUND(SUM(m.paySalaryNet), 2)    AS 'Gasto Neto (€)',
                    ROUND(AVG(m.payBonusApplied), 2) AS 'Bonus Medio (%)'
                FROM MonthlyPayments m
                INNER JOIN ({MonthlyPayment.PaymentOfficeSql}) po ON po.idMonthlyPayment = m.idMonthlyPayment
                INNER JOIN Offices   o ON po.posOffice     = o.idOffice
                INNER JOIN Countries c ON o.offCountry     = c.idCountry
                GROUP BY c.idCountry, c.couName, m.payYear, m.payMonth
                ORDER BY c.couName, m.payYear DESC, m.payMonth DESC;";

            return ToTable(sql,
                ("Código País", typeof(string)), ("País", typeof(string)), ("Año", typeof(int)), ("Mes", typeof(int)),
                ("Empleados", typeof(int)), ("Gasto Bruto (€)", typeof(decimal)), ("Gasto Neto (€)", typeof(decimal)),
                ("Bonus Medio (%)", typeof(decimal)));
        }

        /// <summary>
        /// Construye la tabla del informe: AgenteBD devuelve solo valores, sin nombres ni tipos de columna.
        /// </summary>
        private static DataTable ToTable(string sql, params (string Name, Type Type)[] columns)
        {
            var table = new DataTable();
            foreach (var column in columns)
            {
                table.Columns.Add(column.Name, column.Type);
            }

            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                DataRow dataRow = table.NewRow();
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i].Type == typeof(int)) dataRow[i] = ReadInt(row[i]);
                    else if (columns[i].Type == typeof(decimal)) dataRow[i] = ReadDecimal(row[i]);
                    else dataRow[i] = row[i];
                }
                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}
