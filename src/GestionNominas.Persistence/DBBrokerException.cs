using System;
using MySqlConnector;

namespace GestionNominas.Persistence
{
    /// <summary>
    /// Excepción personalizada para errores ocurridos en la capa de persistencia y Database Broker.
    /// Encapsula las excepciones de bajo nivel de MySQL (MySqlException), traduciéndolas a mensajes
    /// legibles y comprensibles para el usuario y las capas superiores (Dominio y Presentación).
    /// </summary>
    public class DBBrokerException : Exception
    {
        /// <summary>
        /// Código numérico de error nativo de MySQL (ej. 1062 para duplicados, 1451 para restricción foránea).
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Sentencia SQL que provocó el fallo (para depuración y trazabilidad).
        /// </summary>
        public string? SqlQuery { get; }

        public DBBrokerException(string message) : base(message)
        {
        }

        public DBBrokerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DBBrokerException(string message, int errorCode, string? sqlQuery, Exception? innerException = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            SqlQuery = sqlQuery;
        }

        /// <summary>
        /// Analiza una excepción MySqlException y genera una DBBrokerException con un mensaje amigable
        /// según la causa de integridad referencial, duplicidad o conectividad.
        /// </summary>
        public static DBBrokerException FromMySqlException(MySqlException ex, string? sql = null)
        {
            string friendlyMessage;

            switch (ex.Number)
            {
                case 1062: // ER_DUP_ENTRY: Clave duplicada (PK o UNIQUE)
                    friendlyMessage = "Ya existe un registro con los mismos datos clave en la base de datos (clave duplicada).";
                    break;

                case 1451: // ER_ROW_IS_REFERENCED_2: No se puede eliminar por clave foránea
                    friendlyMessage = "No es posible eliminar o modificar este registro porque existen otros datos asociados que dependen de él (integridad referencial).";
                    break;

                case 1452: // ER_NO_REFERENCED_ROW_2: Clave foránea no encontrada
                    friendlyMessage = "No se puede completar la operación: el elemento relacionado especificado no existe en la base de datos.";
                    break;

                case 1042: // ER_BAD_HOST_ERROR
                case 2003: // CR_CONN_HOST_ERROR: No se puede conectar al host
                case 0:    // Error de red / socket
                    friendlyMessage = "No se pudo establecer conexión con el servidor MySQL. Verifique que el servicio esté activo y los datos de conexión sean correctos.";
                    break;

                case 1045: // ER_ACCESS_DENIED_ERROR: Credenciales incorrectas
                    friendlyMessage = "Acceso denegado al servidor MySQL. Compruebe el usuario y contraseña configurados.";
                    break;

                case 1146: // ER_NO_SUCH_TABLE: Tabla inexistente
                    friendlyMessage = "La tabla solicitada no existe en la base de datos. Ejecute el script SQL de creación de tablas.";
                    break;

                case 3819: // Check constraint violation (MySQL 8.0.16+)
                    friendlyMessage = "Los datos proporcionados no cumplen con las restricciones de rango o formato establecidas en la base de datos.";
                    break;

                default:
                    friendlyMessage = $"Error al operar en la base de datos MySQL (Código {ex.Number}): {ex.Message}";
                    break;
            }

            return new DBBrokerException(friendlyMessage, ex.Number, sql, ex);
        }
    }
}
