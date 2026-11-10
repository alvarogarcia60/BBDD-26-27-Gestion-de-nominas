using System;
using System.Collections.Generic;
using System.Linq;
using GestionNominas.Domain.Services;
using Company.persistence;
using static GestionNominas.Domain.Services.RowReader;

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

        #region Métodos de Persistencia (delegan en CountryDAO)

        /// <summary>
        /// DAO con el SQL de la tabla Countries. La entidad no accede a la base de datos directamente.
        /// </summary>
        private static readonly CountryDAO dao = new CountryDAO();

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
            return dao.Insert(IdCountry.ToUpper().Trim(), Name.Trim(), TaxBase) > 0;
        }

        /// <summary>
        /// Actualiza los datos del país.
        /// </summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdCountry.ToUpper().Trim(), Name.Trim(), TaxBase) > 0;
        }

        /// <summary>
        /// Elimina el país si no tiene oficinas ni empleados asociados.
        /// </summary>
        public bool Delete()
        {
            if (dao.CountDependents(IdCountry) > 0)
            {
                throw new InvalidOperationException("El país tiene oficinas o empleados asociados.");
            }

            return dao.Delete(IdCountry) > 0;
        }

        /// <summary>
        /// Lee todos los países ordenados alfabéticamente.
        /// </summary>
        public static List<Country> ReadAll()
        {
            var list = new List<Country>();
            foreach (string[] row in dao.ReadAll())
            {
                list.Add(FromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Busca un país por su clave primaria.
        /// </summary>
        public static Country? Read(string idCountry)
        {
            List<string[]> rows = dao.Read(idCountry);
            return rows.Count > 0 ? FromRow(rows[0]) : null;
        }

        /// <summary>
        /// Comprueba si ya existe un país con el identificador dado.
        /// </summary>
        public static bool Exists(string idCountry)
        {
            return dao.Count(idCountry) > 0;
        }

        /// <summary>
        /// Crea el objeto a partir de una fila del DAO: idCountry, couName, couTaxBase.
        /// </summary>
        private static Country FromRow(string[] row)
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
