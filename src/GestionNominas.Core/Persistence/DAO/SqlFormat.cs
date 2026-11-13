using System;
using System.Globalization;

namespace Company.persistence
{
    /// <summary>
    /// Conversión de valores entre los DAO y AgenteBD, que solo acepta SQL en texto
    /// (sin parámetros) y devuelve cada valor como texto.
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

        /// <summary>Número leído de AgenteBD, que lo devuelve en el formato regional de Windows.</summary>
        public static decimal ReadDecimal(string value) => decimal.Parse(value, CultureInfo.CurrentCulture);

        /// <summary>Número entero leído de AgenteBD.</summary>
        public static int ReadInt(string value) => (int)ReadDecimal(value);

        /// <summary>Fecha leída de AgenteBD.</summary>
        public static DateTime ReadDate(string value) => DateTime.Parse(value, CultureInfo.CurrentCulture);

        /// <summary>Fecha o null: un NULL de la base de datos llega como texto vacío.</summary>
        public static DateTime? ReadDateOrNull(string value) =>
            string.IsNullOrEmpty(value) ? null : ReadDate(value);
    }
}
