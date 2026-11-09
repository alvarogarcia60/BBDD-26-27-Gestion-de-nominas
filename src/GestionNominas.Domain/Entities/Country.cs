using System;
using System.Collections.Generic;
using System.Linq;
using GestionNominas.Domain.Services;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

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

        #region Métodos de Persistencia (AgenteBD)

        /// <summary>
        /// Inserta el país. Si no tiene código, lo genera según la regla del enunciado.
        /// </summary>
        public bool Insert()
        {
            if (string.IsNullOrWhiteSpace(IdCountry))
            {
                IdCountry = CountryCodeGenerator.GenerateCode(Name);
            }

            Validate();

            string sql = $"INSERT INTO Countries (idCountry, couName, couTaxBase) " +
                         $"VALUES ({SqlText(IdCountry.ToUpper().Trim())}, {SqlText(Name.Trim())}, {SqlNumber(TaxBase)});";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Actualiza los datos del país.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = $"UPDATE Countries SET couName = {SqlText(Name.Trim())}, couTaxBase = {SqlNumber(TaxBase)} " +
                         $"WHERE idCountry = {SqlText(IdCountry.ToUpper().Trim())};";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Elimina el país si no tiene oficinas ni empleados asociados.
        /// </summary>
        public bool Delete()
        {
            string id = SqlText(IdCountry);
            string countSql = $"SELECT (SELECT COUNT(*) FROM Offices WHERE offCountry = {id}) + " +
                              $"(SELECT COUNT(*) FROM Employees WHERE empCountry = {id});";

            if (ReadInt(AgenteBD.GetInstance().Read(countSql)[0][0]) > 0)
            {
                throw new InvalidOperationException("El país tiene oficinas o empleados asociados.");
            }

            return AgenteBD.GetInstance().Change($"DELETE FROM Countries WHERE idCountry = {id};") > 0;
        }

        /// <summary>
        /// Lee todos los países ordenados alfabéticamente.
        /// </summary>
        public static List<Country> ReadAll()
        {
            var list = new List<Country>();
            foreach (string[] row in AgenteBD.GetInstance().Read("SELECT idCountry, couName, couTaxBase FROM Countries ORDER BY couName ASC;"))
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca un país por su clave primaria.
        /// </summary>
        public static Country? Read(string idCountry)
        {
            string sql = $"SELECT idCountry, couName, couTaxBase FROM Countries WHERE idCountry = {SqlText(idCountry)};";
            List<string[]> rows = AgenteBD.GetInstance().Read(sql);
            return rows.Count > 0 ? MapFromRow(rows[0]) : null;
        }

        /// <summary>
        /// Comprueba si ya existe un país con el identificador dado.
        /// </summary>
        public static bool Exists(string idCountry)
        {
            string sql = $"SELECT COUNT(*) FROM Countries WHERE idCountry = {SqlText(idCountry)};";
            return ReadInt(AgenteBD.GetInstance().Read(sql)[0][0]) > 0;
        }

        private static Country MapFromRow(string[] row)
        {
            return new Country
            {
                IdCountry = row[0],
                Name = row[1],
                TaxBase = ReadDecimal(row[2])
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
