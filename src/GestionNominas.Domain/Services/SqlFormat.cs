using System;
using System.Globalization;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Conversión de valores entre el dominio y las sentencias de texto que recibe AgenteBD.
    /// AgenteBD no admite parámetros y devuelve cada fila como un array de cadenas.
    /// </summary>
    internal static class SqlFormat
    {
        /// <summary>Literal de texto SQL, escapando comillas y barras para no romper la sentencia.</summary>
        public static string SqlText(string value) =>
            "'" + value.Replace("\\", "\\\\").Replace("'", "''") + "'";

        /// <summary>Número con punto decimal, independientemente del idioma de Windows.</summary>
        public static string SqlNumber(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        public static string SqlDate(DateTime value) =>
            "'" + value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";

        public static string SqlDateOrNull(DateTime? value) => value.HasValue ? SqlDate(value.Value) : "NULL";

        // AgenteBD convierte cada valor con ToString(), que usa el formato regional actual.
        public static decimal ReadDecimal(string value) => decimal.Parse(value, CultureInfo.CurrentCulture);

        public static int ReadInt(string value) => (int)ReadDecimal(value);

        public static DateTime ReadDate(string value) => DateTime.Parse(value, CultureInfo.CurrentCulture);

        /// <summary>Un valor NULL de la base de datos llega como cadena vacía.</summary>
        public static DateTime? ReadDateOrNull(string value) =>
            string.IsNullOrEmpty(value) ? null : ReadDate(value);
    }
}
