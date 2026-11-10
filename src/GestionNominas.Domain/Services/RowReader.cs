using System;
using System.Globalization;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Convierte los valores de las filas que devuelven los DAO (cadenas) a tipos del dominio.
    /// Los valores llegan como texto en el formato regional actual.
    /// </summary>
    internal static class RowReader
    {
        public static decimal ReadDecimal(string value) => decimal.Parse(value, CultureInfo.CurrentCulture);

        public static int ReadInt(string value) => (int)ReadDecimal(value);

        public static DateTime ReadDate(string value) => DateTime.Parse(value, CultureInfo.CurrentCulture);

        /// <summary>Un valor NULL de la base de datos llega como cadena vacía.</summary>
        public static DateTime? ReadDateOrNull(string value) =>
            string.IsNullOrEmpty(value) ? null : ReadDate(value);
    }
}
