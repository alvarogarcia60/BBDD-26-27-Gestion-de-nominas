using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace GestionNominas.Persistence
{
    /// <summary>
    /// Patrón Database Broker (Broker de Persistencia).
    /// Centraliza y encapsula todas las operaciones de acceso a la base de datos MySQL,
    /// abstrayendo al resto de capas (Dominio y Presentación) de los detalles del driver SQL,
    /// gestión de conexiones, pooling y control transaccional.
    /// Implementa el patrón creacional Singleton garantizando un único punto de acceso seguro.
    /// </summary>
    public sealed class DBBroker
    {
        private static DBBroker? _instance;
        private static readonly object _lock = new();

        private MySqlConnection? _connection;
        private MySqlTransaction? _transaction;

        /// <summary>
        /// Constructor privado para impedir la instanciación directa externa (Patrón Singleton).
        /// </summary>
        private DBBroker()
        {
        }

        /// <summary>
        /// Obtiene la instancia única del Broker de Base de Datos (Thread-Safe con Double-Checked Locking).
        /// </summary>
        public static DBBroker GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DBBroker();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Comprueba y abre la conexión con el servidor MySQL si no está activa.
        /// </summary>
        private void EnsureConnectionOpen()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                }

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex);
            }
            catch (Exception ex)
            {
                throw new DBBrokerException($"Error inesperado al conectar con MySQL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si la conexión con la base de datos es exitosa en el arranque del sistema.
        /// Captura cualquier error para evitar el cierre inesperado de la aplicación.
        /// </summary>
        /// <param name="errorMessage">Mensaje descriptivo si ocurre algún fallo de conexión.</param>
        /// <returns>True si la conexión es exitosa, false en caso contrario.</returns>
        public bool TestConnection(out string errorMessage)
        {
            try
            {
                using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
                conn.Open();
                errorMessage = string.Empty;
                return true;
            }
            catch (MySqlException ex)
            {
                var brokerEx = DBBrokerException.FromMySqlException(ex);
                errorMessage = brokerEx.Message;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Fallo general de conexión: {ex.Message}";
                return false;
            }
        }

        #region Control Transaccional ACID

        /// <summary>
        /// Inicia una transacción explícita en la base de datos.
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                EnsureConnectionOpen();
                if (_transaction == null)
                {
                    _transaction = _connection!.BeginTransaction();
                }
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, "BEGIN TRANSACTION");
            }
        }

        /// <summary>
        /// Confirma todos los cambios realizados en la transacción activa.
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Commit();
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, "COMMIT TRANSACTION");
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        /// <summary>
        /// Revierte todos los cambios de la transacción activa en caso de error.
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, "ROLLBACK TRANSACTION");
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        #endregion

        #region Métodos de Ejecución Parametrizados (CRUD)

        /// <summary>
        /// Ejecuta una consulta SELECT y devuelve el resultado en una estructura DataTable.
        /// Utiliza consultas parametrizadas para inmunidad frente a Inyecciones SQL.
        /// </summary>
        /// <param name="sql">Sentencia SQL SELECT a ejecutar.</param>
        /// <param name="parameters">Diccionario opcional de parámetros SQL.</param>
        /// <returns>DataTable con las filas y columnas devueltas por la consulta.</returns>
        public DataTable ExecuteQuery(string sql, Dictionary<string, object?>? parameters = null)
        {
            var dataTable = new DataTable();

            try
            {
                EnsureConnectionOpen();

                using var command = new MySqlCommand(sql, _connection, _transaction);
                AddParameters(command, parameters);

                using var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataTable);

                return dataTable;
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, sql);
            }
            catch (Exception ex) when (ex is not DBBrokerException)
            {
                throw new DBBrokerException($"Error durante la lectura de datos: {ex.Message}", ex);
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        /// <summary>
        /// Ejecuta instrucciones de modificación de datos (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Sentencia SQL a ejecutar.</param>
        /// <param name="parameters">Diccionario opcional de parámetros SQL.</param>
        /// <returns>Número de filas afectadas por la sentencia.</returns>
        public int ExecuteNonQuery(string sql, Dictionary<string, object?>? parameters = null)
        {
            try
            {
                EnsureConnectionOpen();

                using var command = new MySqlCommand(sql, _connection, _transaction);
                AddParameters(command, parameters);

                return command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, sql);
            }
            catch (Exception ex) when (ex is not DBBrokerException)
            {
                throw new DBBrokerException($"Error durante la modificación de datos: {ex.Message}", ex);
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        /// <summary>
        /// Ejecuta una instrucción INSERT y devuelve atómicamente el identificador autonumérico generado (LAST_INSERT_ID()).
        /// </summary>
        /// <param name="sql">Sentencia INSERT a ejecutar.</param>
        /// <param name="parameters">Diccionario opcional de parámetros SQL.</param>
        /// <returns>ID autoincremental generado.</returns>
        public long ExecuteInsertAndGetId(string sql, Dictionary<string, object?>? parameters = null)
        {
            try
            {
                EnsureConnectionOpen();

                // Concatenamos SELECT LAST_INSERT_ID(); para asegurar atomicidad dentro de la misma sesión
                string commandText = sql.TrimEnd(';') + "; SELECT LAST_INSERT_ID();";

                using var command = new MySqlCommand(commandText, _connection, _transaction);
                AddParameters(command, parameters);

                object? result = command.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToInt64(result) : 0;
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, sql);
            }
            catch (Exception ex) when (ex is not DBBrokerException)
            {
                throw new DBBrokerException($"Error durante la inserción con generación de ID: {ex.Message}", ex);
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        /// <summary>
        /// Ejecuta una consulta que devuelve un único valor escalar (ej. COUNT(*), MAX(id), etc.).
        /// </summary>
        /// <param name="sql">Sentencia SQL a ejecutar.</param>
        /// <param name="parameters">Diccionario opcional de parámetros SQL.</param>
        /// <returns>Valor escalar resultante o null.</returns>
        public object? ExecuteScalar(string sql, Dictionary<string, object?>? parameters = null)
        {
            try
            {
                EnsureConnectionOpen();

                using var command = new MySqlCommand(sql, _connection, _transaction);
                AddParameters(command, parameters);

                return command.ExecuteScalar();
            }
            catch (MySqlException ex)
            {
                throw DBBrokerException.FromMySqlException(ex, sql);
            }
            catch (Exception ex) when (ex is not DBBrokerException)
            {
                throw new DBBrokerException($"Error durante la consulta escalar: {ex.Message}", ex);
            }
            finally
            {
                CloseConnectionIfNotInTransaction();
            }
        }

        #endregion

        #region Métodos Privados Auxiliares

        /// <summary>
        /// Añade de forma segura los parámetros al comando MySqlCommand previniendo inyección SQL.
        /// </summary>
        private static void AddParameters(MySqlCommand command, Dictionary<string, object?>? parameters)
        {
            if (parameters == null) return;

            foreach (var kvp in parameters)
            {
                string paramName = kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key;
                object? value = kvp.Value ?? DBNull.Value;
                command.Parameters.AddWithValue(paramName, value);
            }
        }

        /// <summary>
        /// Cierra la conexión física si no existe una transacción activa en curso.
        /// </summary>
        private void CloseConnectionIfNotInTransaction()
        {
            if (_transaction == null && _connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        #endregion
    }
}
