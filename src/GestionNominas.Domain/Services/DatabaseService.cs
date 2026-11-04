using GestionNominas.Persistence;

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
            return DBBroker.GetInstance().TestConnection(out errorMessage);
        }

        /// <summary>
        /// Descripción legible del destino de conexión (servidor, puerto, base de datos y usuario).
        /// </summary>
        public static string ConnectionDescription =>
            $"Servidor: {DatabaseConfig.Server}\n" +
            $"Puerto: {DatabaseConfig.Port}\n" +
            $"Base de datos: {DatabaseConfig.Database}\n" +
            $"Usuario: {DatabaseConfig.UserId}";

        /// <summary>
        /// Nombre de la base de datos y servidor, en una sola línea.
        /// </summary>
        public static string ShortDescription =>
            $"{DatabaseConfig.Database} en {DatabaseConfig.Server}:{DatabaseConfig.Port}";
    }
}
