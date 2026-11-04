using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GestionNominas.Domain.Services;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: Country (País).
    /// Mapea la tabla 'Countries' del esquema relacional.
    /// Representa un país con su código identificador ISO de 3 caracteres,
    /// denominación y porcentaje base de deducción mensual por impuestos (couTaxBase).
    /// </summary>
    public class Country
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Código identificador del país (Clave Primaria, CHAR(3), ej. 'ESP', 'FRA').
        /// </summary>
        public string IdCountry { get; set; } = string.Empty;

        /// <summary>
        /// Nombre oficial o común del país (VARCHAR(120)).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Porcentaje mensual de impuestos aplicable en el país (DECIMAL(4,2), ej. 15.00%).
        /// </summary>
        public decimal TaxBase { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para serialización, instanciación genérica y binding.
        /// </summary>
        public Country()
        {
        }

        /// <summary>
        /// Constructor parametrizado completo.
        /// </summary>
        /// <param name="idCountry">Código de 3 caracteres del país.</param>
        /// <param name="name">Nombre del país.</param>
        /// <param name="taxBase">Porcentaje de retención impositiva (0 - 100).</param>
        public Country(string idCountry, string name, decimal taxBase)
        {
            IdCountry = idCountry;
            Name = name;
            TaxBase = taxBase;
        }

        #endregion

        #region Métodos de Persistencia (Patrón Database Broker)

        /// <summary>
        /// Inserta el país actual en la base de datos a través del Database Broker.
        /// Si el código IdCountry no está especificado, se genera automáticamente siguiendo
        /// la regla de negocio de las 3 primeras letras y resolución de colisiones.
        /// </summary>
        public bool Insert()
        {
            if (string.IsNullOrWhiteSpace(IdCountry))
            {
                IdCountry = CountryCodeGenerator.GenerateCode(Name);
            }

            Validate();

            string sql = "INSERT INTO Countries (idCountry, couName, couTaxBase) VALUES (@idCountry, @couName, @couTaxBase);";
            var parameters = new Dictionary<string, object?>
            {
                { "@idCountry", IdCountry.ToUpper().Trim() },
                { "@couName", Name.Trim() },
                { "@couTaxBase", TaxBase }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Actualiza los datos del país en la base de datos.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = "UPDATE Countries SET couName = @couName, couTaxBase = @couTaxBase WHERE idCountry = @idCountry;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idCountry", IdCountry.ToUpper().Trim() },
                { "@couName", Name.Trim() },
                { "@couTaxBase", TaxBase }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Elimina el país de la base de datos.
        /// </summary>
        public bool Delete()
        {
            string sql = "DELETE FROM Countries WHERE idCountry = @idCountry;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idCountry", IdCountry }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Lee y mapea todos los países registrados ordenados alfabéticamente.
        /// </summary>
        public static List<Country> ReadAll()
        {
            string sql = "SELECT idCountry, couName, couTaxBase FROM Countries ORDER BY couName ASC;";
            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql);

            var list = new List<Country>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca un país por su clave primaria (código de 3 caracteres).
        /// </summary>
        public static Country? Read(string idCountry)
        {
            string sql = "SELECT idCountry, couName, couTaxBase FROM Countries WHERE idCountry = @idCountry;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idCountry", idCountry }
            };

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapFromRow(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Comprueba si ya existe un país registrado con el identificador dado.
        /// </summary>
        public static bool Exists(string idCountry)
        {
            string sql = "SELECT COUNT(*) FROM Countries WHERE idCountry = @idCountry;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idCountry", idCountry }
            };

            object? count = DBBroker.GetInstance().ExecuteScalar(sql, parameters);
            return count != null && Convert.ToInt32(count) > 0;
        }

        /// <summary>
        /// Mapea una fila de datos (DataRow) a una instancia de Country.
        /// </summary>
        private static Country MapFromRow(DataRow row)
        {
            return new Country
            {
                IdCountry = row["idCountry"].ToString() ?? string.Empty,
                Name = row["couName"].ToString() ?? string.Empty,
                TaxBase = Convert.ToDecimal(row["couTaxBase"])
            };
        }

        #endregion

        #region Reglas de Validación de Negocio

        /// <summary>
        /// Valida la consistencia de los datos del país.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(IdCountry) || IdCountry.Trim().Length != 3 || !IdCountry.Trim().All(char.IsLetter))
            {
                throw new ArgumentException("El código del país debe tener exactamente 3 caracteres alfabéticos.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("El nombre del país no puede estar vacío.");
            }

            if (TaxBase < 0 || TaxBase > 99.99m)
            {
                throw new ArgumentOutOfRangeException(nameof(TaxBase), "La base impositiva debe estar comprendida entre 0.00% y 99.99%.");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{Name} ({IdCountry})";
        }
    }
}
