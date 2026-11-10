using System;
using System.Globalization;

namespace Company.persistence
{
    /// <summary>
    /// Escribe valores dentro de las sentencias SQL que reciben los DAO.
    /// AgenteBD solo acepta texto SQL (sin parámetros), así que los valores se escapan aquí.
    /// </summary>
    internal static class SqlFormat
    {
        /// <summary>Literal de texto SQL, escapando comillas y barras para no romper la sentencia.</summary>
        public static string SqlText(string value) =>
            "'" + value.Replace("\\", "\\\\").Replace("'", "''") + "'";

        /// <summary>Número con punto decimal, independientemente del idioma de Windows.</summary>
        public static string SqlNumber(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>Fecha en formato 'yyyy-MM-dd'.</summary>
        public static string SqlDate(DateTime value) =>
            "'" + value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";

        /// <summary>Fecha o NULL si no tiene valor.</summary>
        public static string SqlDateOrNull(DateTime? value) => value.HasValue ? SqlDate(value.Value) : "NULL";

        /// <summary>
        /// Lee el resultado de un SELECT COUNT(*). AgenteBD devuelve los valores con ToString(),
        /// que usa el formato regional actual.
        /// </summary>
        public static int ReadCount(string value) => (int)decimal.Parse(value, CultureInfo.CurrentCulture);
    }
}
