using System;
using System.Collections.Generic;
using System.Data;
using GestionNominas.Domain.Entities;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio: Informes agregados de nóminas.
    /// Centraliza las consultas de los informes para que la capa de presentación
    /// no acceda directamente a la capa de persistencia (arquitectura en 3 capas).
    /// Las nóminas se atribuyen a la oficina en la que el empleado trabajó más días
    /// en el mes correspondiente (ver <see cref="MonthlyPayment.PredominantOfficeSql"/>).
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

            return DBBroker.GetInstance().ExecuteQuery(sql);
        }

        /// <summary>
        /// Informe 4: gasto en nóminas de cada oficina en un año-mes.
        /// Cada nómina se cuenta una única vez, en la oficina predominante del empleado ese mes.
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
                INNER JOIN ({MonthlyPayment.PredominantOfficeSql}) po ON po.posEmployee = m.payEmployee
                INNER JOIN Offices   o ON po.posOffice  = o.idOffice
                INNER JOIN Countries c ON o.offCountry  = c.idCountry
                WHERE m.payYear = @year AND m.payMonth = @month
                GROUP BY o.idOffice, o.offCity, c.couName
                ORDER BY SUM(m.paySalaryGross) DESC;";

            var parameters = new Dictionary<string, object?>
            {
                { "@year", year },
                { "@month", month },
                { "@firstDay", new DateTime(year, month, 1) },
                { "@lastDay", new DateTime(year, month, DateTime.DaysInMonth(year, month)) }
            };

            return DBBroker.GetInstance().ExecuteQuery(sql, parameters);
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada año-mes con datos.
        /// </summary>
        public static DataTable GetCountryExpensesByMonth()
        {
            string sql = @"
                SELECT
                    c.idCountry                      AS 'Código País',
                    c.couName                        AS 'País',
                    m.payYear                        AS 'Año',
                    m.payMonth                       AS 'Mes',
                    COUNT(*)                         AS 'Empleados',
                    ROUND(SUM(m.paySalaryGross), 2)  AS 'Gasto Bruto (€)',
                    ROUND(SUM(m.paySalaryNet), 2)    AS 'Gasto Neto (€)',
                    ROUND(AVG(m.payBonusApplied), 2) AS 'Bonus Medio (%)'
                FROM MonthlyPayments m
                /* Oficina predominante del empleado en el mes de cada nómina */
                INNER JOIN (
                    SELECT p.posEmployee, p.posOffice, mp.payYear, mp.payMonth,
                           ROW_NUMBER() OVER (
                               PARTITION BY p.posEmployee, mp.payYear, mp.payMonth
                               ORDER BY DATEDIFF(
                                            LEAST(COALESCE(p.posEndDate, LAST_DAY(MAKEDATE(mp.payYear, 1) + INTERVAL (mp.payMonth - 1) MONTH)),
                                                  LAST_DAY(MAKEDATE(mp.payYear, 1) + INTERVAL (mp.payMonth - 1) MONTH)),
                                            GREATEST(p.posStartDate, MAKEDATE(mp.payYear, 1) + INTERVAL (mp.payMonth - 1) MONTH)
                                        ) DESC,
                                        p.posStartDate DESC
                           ) AS rn
                    FROM Positions p
                    INNER JOIN MonthlyPayments mp ON mp.payEmployee = p.posEmployee
                    WHERE p.posStartDate <= LAST_DAY(MAKEDATE(mp.payYear, 1) + INTERVAL (mp.payMonth - 1) MONTH)
                      AND (p.posEndDate IS NULL
                           OR p.posEndDate >= MAKEDATE(mp.payYear, 1) + INTERVAL (mp.payMonth - 1) MONTH)
                ) AS ranked
                    ON  ranked.posEmployee = m.payEmployee
                    AND ranked.payYear     = m.payYear
                    AND ranked.payMonth    = m.payMonth
                    AND ranked.rn          = 1
                INNER JOIN Offices   o ON ranked.posOffice = o.idOffice
                INNER JOIN Countries c ON o.offCountry     = c.idCountry
                GROUP BY c.idCountry, c.couName, m.payYear, m.payMonth
                ORDER BY c.couName, m.payYear DESC, m.payMonth DESC;";

            return DBBroker.GetInstance().ExecuteQuery(sql);
        }
    }
}
