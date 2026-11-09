using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

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

        #region Métodos de Persistencia (AgenteBD)

        private const string SelectSql = @"SELECT o.idOffice, o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                                  c.couName, c.couTaxBase
                                           FROM Offices o
                                           INNER JOIN Countries c ON o.offCountry = c.idCountry";

        /// <summary>
        /// Inserta la oficina.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = $"INSERT INTO Offices (offCountry, offCity, offAddress, offThreeYearBonus) " +
                         $"VALUES ({SqlText(Country.IdCountry)}, {SqlText(City.Trim())}, {SqlText(Address.Trim())}, {SqlNumber(ThreeYearBonus)});";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Actualiza los datos de la oficina.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = $"UPDATE Offices SET offCountry = {SqlText(Country.IdCountry)}, offCity = {SqlText(City.Trim())}, " +
                         $"offAddress = {SqlText(Address.Trim())}, offThreeYearBonus = {SqlNumber(ThreeYearBonus)} " +
                         $"WHERE idOffice = {IdOffice};";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Elimina la oficina si no tiene puestos asociados.
        /// </summary>
        public bool Delete()
        {
            string countSql = $"SELECT COUNT(*) FROM Positions WHERE posOffice = {IdOffice};";
            if (ReadInt(AgenteBD.GetInstance().Read(countSql)[0][0]) > 0)
            {
                throw new InvalidOperationException("La oficina tiene puestos de trabajo asociados.");
            }

            return AgenteBD.GetInstance().Change($"DELETE FROM Offices WHERE idOffice = {IdOffice};") > 0;
        }

        /// <summary>
        /// Lee todas las oficinas cargando sus objetos Country.
        /// </summary>
        public static List<Office> ReadAll()
        {
            var list = new List<Office>();
            foreach (string[] row in AgenteBD.GetInstance().Read(SelectSql + " ORDER BY c.couName ASC, o.offCity ASC;"))
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca una oficina por su identificador.
        /// </summary>
        public static Office? Read(int idOffice)
        {
            List<string[]> rows = AgenteBD.GetInstance().Read($"{SelectSql} WHERE o.idOffice = {idOffice};");
            return rows.Count > 0 ? MapFromRow(rows[0]) : null;
        }

        private static Office MapFromRow(string[] row)
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
