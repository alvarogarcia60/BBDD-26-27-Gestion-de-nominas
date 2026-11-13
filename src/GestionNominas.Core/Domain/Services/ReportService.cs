using System;
using System.Data;
using Company.persistence;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Informes 3, 4 y 5: los pide a ReportDAO y los devuelve a la presentación.
    /// </summary>
    public static class ReportService
    {
        /// <summary>DAO con las consultas de los informes.</summary>
        private static readonly ReportDAO dao = new ReportDAO();

        /// <summary>
        /// Informe 3: empleados de cada sede (oficina de su puesto actual) con los trienios
        /// completos acumulados hasta hoy en cualquier puesto.
        /// </summary>
        public static DataTable GetEmployeesWithTrieniosByOffice()
        {
            dao.EmployeesWithTrieniosByOffice();
            return dao.Table;
        }

        /// <summary>
        /// Informe 4: gasto en nóminas de cada oficina en un año y mes.
        /// Si un empleado cambió de oficina, cada nómina cuenta en su oficina.
        /// </summary>
        public static DataTable GetOfficeExpensesByMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "El mes debe estar comprendido entre 1 y 12.");

            dao.OfficeExpensesByMonth(year, month);
            return dao.Table;
        }

        /// <summary>
        /// Informe 5: gasto en nóminas de cada país (país de la oficina) en cada mes con datos.
        /// </summary>
        public static DataTable GetCountryExpensesByMonth()
        {
            dao.CountryExpensesByMonth();
            return dao.Table;
        }
    }
}
