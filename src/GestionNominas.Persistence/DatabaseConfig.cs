using System;

namespace GestionNominas.Persistence
{
    /// <summary>
    /// Configuración de los parámetros de conexión al servidor MySQL.
    /// Permite centralizar la configuración de conexión y modificarla en tiempo de ejecución.
    /// </summary>
    public static class DatabaseConfig
    {
        public static string Server { get; set; } = "localhost";
        public static uint Port { get; set; } = 3306;
        public static string Database { get; set; } = "nominas_db";
        public static string UserId { get; set; } = "root";
        public static string Password { get; set; } = "root";
        public static string CharacterSet { get; set; } = "utf8mb4";
        public static bool SslMode { get; set; } = false;

        /// <summary>
        /// Construye y devuelve la cadena de conexión formateada para MySQL Connector.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return $"Server={Server};Port={Port};Database={Database};Uid={UserId};Pwd={Password};CharSet={CharacterSet};SslMode={(SslMode ? "Required" : "None")};AllowUserVariables=True;";
            }
        }
    }
}
