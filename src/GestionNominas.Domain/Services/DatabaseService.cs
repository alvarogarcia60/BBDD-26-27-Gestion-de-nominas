using System;
using Company.persistence;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio: fachada sobre la conexión a la base de datos, para que la
    /// capa de presentación no dependa directamente de la capa de persistencia.
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>
        /// Comprueba si es posible conectar con la base de datos.
        /// </summary>
        /// <param name="errorMessage">Mensaje descriptivo del fallo, si lo hay.</param>
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

        /// <summary>
        /// Descripción legible del destino de conexión (servidor, base de datos y usuario) de la capa de persistencia.
        /// </summary>
        public static string ConnectionDescription => "Servidor: localhost\nBase de datos: Company\nUsuario: root";

        /// <summary>
        /// Nombre de la base de datos y servidor, en una sola línea.
        /// </summary>
        public static string ShortDescription => "Company en localhost";
    }
}
