using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.RowReader;

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

        #region Métodos de Persistencia (delegan en OfficeDAO)

        /// <summary>
        /// DAO con el SQL de la tabla Offices. La entidad no accede a la base de datos directamente.
        /// </summary>
        private static readonly OfficeDAO dao = new OfficeDAO();

        /// <summary>
        /// Inserta la oficina.
        /// </summary>
        public bool Insert()
        {
            Validate();
            return dao.Insert(Country.IdCountry, City.Trim(), Address.Trim(), ThreeYearBonus) > 0;
        }

        /// <summary>
        /// Actualiza los datos de la oficina.
        /// </summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdOffice, Country.IdCountry, City.Trim(), Address.Trim(), ThreeYearBonus) > 0;
        }

        /// <summary>
        /// Elimina la oficina si no tiene puestos asociados.
        /// </summary>
        public bool Delete()
        {
            if (dao.CountPositions(IdOffice) > 0)
            {
                throw new InvalidOperationException("La oficina tiene puestos de trabajo asociados.");
            }

            return dao.Delete(IdOffice) > 0;
        }

        /// <summary>
        /// Lee todas las oficinas cargando sus objetos Country.
        /// </summary>
        public static List<Office> ReadAll()
        {
            var list = new List<Office>();
            foreach (string[] row in dao.ReadAll())
            {
                list.Add(FromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca una oficina por su identificador.
        /// </summary>
        public static Office? Read(int idOffice)
        {
            List<string[]> rows = dao.Read(idOffice);
            return rows.Count > 0 ? FromRow(rows[0]) : null;
        }

        /// <summary>
        /// Crea el objeto a partir de una fila del DAO, incluido su objeto Country.
        /// </summary>
        private static Office FromRow(string[] row)
        {
            return new Office
            {
                IdOffice = ReadInt(row[0]),
                Country = new Country { IdCountry = row[1], Name = row[5], TaxBase = ReadDecimal(row[6]) },
                City = row[2],
                Address = row[3],
                ThreeYearBonus = ReadDecimal(row[4])
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

            if (ThreeYearBonus < 0 || ThreeYearBonus > 99.99m)
            {
                throw new ArgumentOutOfRangeException(nameof(ThreeYearBonus), "El porcentaje de bonus por trienio debe estar entre 0.00% y 99.99%.");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{City} ({Country?.IdCountry}) - {Address}";
        }
    }
}
