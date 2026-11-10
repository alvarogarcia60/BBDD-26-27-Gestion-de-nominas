using System;
using System.Collections.Generic;
using System.Data;
using Company.persistence;
using static GestionNominas.Domain.Services.RowReader;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio: Informes agregados de nóminas.
    /// Pide los datos de los informes 3, 4 y 5 a ReportDAO y los devuelve como tablas
    /// para que la capa de presentación no acceda a la capa de persistencia (arquitectura en 3 capas).
    /// </summary>
    public static class ReportService
    {
        /// <summary>
        /// DAO con las consultas SQL de los informes.
        /// </summary>
        private static readonly ReportDAO dao = new ReportDAO();

        /// <summary>
        /// Informe 3: empleados de cada sede (oficina del puesto actual) con los trienios
        /// completos acumulados hasta hoy en cualquier puesto.
        /// </summary>
        public static DataTable GetEmployeesWithTrieniosByOffice()
        {
            return ToTable(dao.EmployeesWithTrieniosByOffice(),
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

            return ToTable(dao.OfficeExpensesByMonth(year, month),
                ("ID Oficina", typeof(int)), ("Ciudad", typeof(string)), ("País", typeof(string)), ("Empleados", typeof(int)),
                ("Gasto Bruto Total (€)", typeof(decimal)), ("Gasto Neto Total (€)", typeof(decimal)));
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada año-mes con datos.
        /// </summary>
        public static DataTable GetCountryExpensesByMonth()
        {
            return ToTable(dao.CountryExpensesByMonth(),
                ("Código País", typeof(string)), ("País", typeof(string)), ("Año", typeof(int)), ("Mes", typeof(int)),
                ("Empleados", typeof(int)), ("Gasto Bruto (€)", typeof(decimal)), ("Gasto Neto (€)", typeof(decimal)),
                ("Bonus Medio (%)", typeof(decimal)));
        }

        /// <summary>
        /// Construye la tabla del informe: el DAO devuelve solo valores, sin nombres ni tipos de columna.
        /// </summary>
        private static DataTable ToTable(List<string[]> rows, params (string Name, Type Type)[] columns)
        {
            var table = new DataTable();
            foreach (var column in columns)
            {
                table.Columns.Add(column.Name, column.Type);
            }

            foreach (string[] row in rows)
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
