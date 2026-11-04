using System;
using System.Collections.Generic;
using System.Data;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: Office (Oficina / Sede).
    /// Mapea la tabla 'Offices' del esquema relacional.
    /// Representa una sede de la multinacional en una ciudad y país determinados.
    /// Aplica estrictamente los principios de POO: la clave foránea offCountry se instancia
    /// como una referencia directa al objeto de dominio Country, no como un tipo primitivo.
    /// </summary>
    public class Office
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Identificador único autonumérico de la oficina (Clave Primaria, INT).
        /// </summary>
        public int IdOffice { get; set; }

        /// <summary>
        /// Relación de Dominio POO: Objeto Country al que pertenece la oficina.
        /// Corresponde a la clave foránea 'offCountry' vinculada a la tabla Countries.
        /// </summary>
        public Country Country { get; set; } = new Country();

        /// <summary>
        /// Ciudad en la que radica la sede (VARCHAR(60)).
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Dirección postal de la oficina (VARCHAR(120)).
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Porcentaje base de bonus por trienio completado aplicable en esta oficina (DECIMAL(4,2)).
        /// </summary>
        public decimal ThreeYearBonus { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para enlaces de datos, serialización y mapeo.
        /// </summary>
        public Office()
        {
        }

        /// <summary>
        /// Constructor parametrizado para creación de nuevas entidades (sin identificador de base de datos).
        /// </summary>
        /// <param name="country">Objeto País al que se asocia la oficina.</param>
        /// <param name="city">Ciudad de ubicación.</param>
        /// <param name="address">Dirección de la sede.</param>
        /// <param name="threeYearBonus">Porcentaje base de bonus por trienio.</param>
        public Office(Country country, string city, string address, decimal threeYearBonus)
        {
            Country = country ?? throw new ArgumentNullException(nameof(country), "El país asociado no puede ser nulo.");
            City = city;
            Address = address;
            ThreeYearBonus = threeYearBonus;
        }

        /// <summary>
        /// Constructor parametrizado completo (para reconstrucción desde la capa de persistencia).
        /// </summary>
        /// <param name="idOffice">Identificador único de la oficina.</param>
        /// <param name="country">Objeto País asociado.</param>
        /// <param name="city">Ciudad de ubicación.</param>
        /// <param name="address">Dirección de la sede.</param>
        /// <param name="threeYearBonus">Porcentaje base de bonus por trienio.</param>
        public Office(int idOffice, Country country, string city, string address, decimal threeYearBonus)
            : this(country, city, address, threeYearBonus)
        {
            IdOffice = idOffice;
        }

        #endregion

        #region Métodos de Persistencia (Patrón Database Broker)

        /// <summary>
        /// Inserta la oficina actual en la base de datos a través del Database Broker.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = @"INSERT INTO Offices (offCountry, offCity, offAddress, offThreeYearBonus) 
                           VALUES (@offCountry, @offCity, @offAddress, @offThreeYearBonus);";

            var parameters = new Dictionary<string, object?>
            {
                { "@offCountry", Country.IdCountry },
                { "@offCity", City.Trim() },
                { "@offAddress", Address.Trim() },
                { "@offThreeYearBonus", ThreeYearBonus }
            };

            long generatedId = DBBroker.GetInstance().ExecuteInsertAndGetId(sql, parameters);
            if (generatedId > 0)
            {
                IdOffice = (int)generatedId;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actualiza los datos de la oficina en la base de datos.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = @"UPDATE Offices 
                           SET offCountry = @offCountry, offCity = @offCity, 
                               offAddress = @offAddress, offThreeYearBonus = @offThreeYearBonus 
                           WHERE idOffice = @idOffice;";

            var parameters = new Dictionary<string, object?>
            {
                { "@idOffice", IdOffice },
                { "@offCountry", Country.IdCountry },
                { "@offCity", City.Trim() },
                { "@offAddress", Address.Trim() },
                { "@offThreeYearBonus", ThreeYearBonus }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Elimina la oficina de la base de datos.
        /// </summary>
        public bool Delete()
        {
            string sql = "DELETE FROM Offices WHERE idOffice = @idOffice;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idOffice", IdOffice }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Lee y mapea todas las oficinas cargando sus objetos Country asociados mediante INNER JOIN.
        /// </summary>
        public static List<Office> ReadAll()
        {
            string sql = @"SELECT o.idOffice, o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                  c.couName, c.couTaxBase
                           FROM Offices o
                           INNER JOIN Countries c ON o.offCountry = c.idCountry
                           ORDER BY c.couName ASC, o.offCity ASC;";

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql);
            var list = new List<Office>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca una oficina por su identificador primario cargando la referencia al objeto Country.
        /// </summary>
        public static Office? Read(int idOffice)
        {
            string sql = @"SELECT o.idOffice, o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                  c.couName, c.couTaxBase
                           FROM Offices o
                           INNER JOIN Countries c ON o.offCountry = c.idCountry
                           WHERE o.idOffice = @idOffice;";

            var parameters = new Dictionary<string, object?>
            {
                { "@idOffice", idOffice }
            };

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapFromRow(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Mapea una fila de datos a un objeto Office instanciando su propiedad Country.
        /// </summary>
        private static Office MapFromRow(DataRow row)
        {
            var country = new Country
            {
                IdCountry = row["offCountry"].ToString() ?? string.Empty,
                Name = row.Table.Columns.Contains("couName") ? row["couName"].ToString() ?? string.Empty : string.Empty,
                TaxBase = row.Table.Columns.Contains("couTaxBase") ? Convert.ToDecimal(row["couTaxBase"]) : 0m
            };

            return new Office
            {
                IdOffice = Convert.ToInt32(row["idOffice"]),
                Country = country,
                City = row["offCity"].ToString() ?? string.Empty,
                Address = row["offAddress"].ToString() ?? string.Empty,
                ThreeYearBonus = Convert.ToDecimal(row["offThreeYearBonus"])
            };
        }

        #endregion

        #region Reglas de Validación de Negocio

        /// <summary>
        /// Valida la integridad y restricciones del dominio para la oficina.
        /// </summary>
        public void Validate()
        {
            if (Country == null || string.IsNullOrWhiteSpace(Country.IdCountry))
            {
                throw new ArgumentException("La oficina debe tener asignado un país válido.");
            }

            if (string.IsNullOrWhiteSpace(City))
            {
                throw new ArgumentException("La ciudad de la oficina no puede estar vacía.");
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new ArgumentException("La dirección de la oficina no puede estar vacía.");
            }

            if (ThreeYearBonus < 0 || ThreeYearBonus > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(ThreeYearBonus), "El porcentaje de bonus por trienio debe estar entre 0.00% y 100.00%.");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{City} ({Country?.IdCountry}) - {Address}";
        }
    }
}
