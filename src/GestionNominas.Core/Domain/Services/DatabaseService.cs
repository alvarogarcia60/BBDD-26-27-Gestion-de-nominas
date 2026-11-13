using System;
using Company.persistence;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Comprobación de la conexión con la base de datos, para que la presentación
    /// no tenga que acceder a la capa de persistencia.
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>Indica si se puede conectar; si no, devuelve el motivo en errorMessage.</summary>
        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                new ConnectionDAO().TestConnection();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>Datos de la conexión, para mostrarlos si falla.</summary>
        public static string ConnectionDescription => "Servidor: localhost\nBase de datos: Company\nUsuario: root";

        /// <summary>Base de datos y servidor, en una sola línea.</summary>
        public static string ShortDescription => "Company en localhost";
    }
}
