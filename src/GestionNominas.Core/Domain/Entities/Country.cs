using System;
using System.Collections.Generic;
using System.Linq;
using GestionNominas.Domain.Services;
using Company.persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// País (tabla Countries), con su porcentaje mensual de impuestos.
    /// </summary>
    public class Country
    {
        #region Propiedades

        /// <summary>Clave primaria: código de 3 letras (ESP, FRA...).</summary>
        public string IdCountry { get; set; } = string.Empty;

        /// <summary>Nombre del país.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Porcentaje mensual de impuestos.</summary>
        public decimal TaxBase { get; set; }

        #endregion

        #region Constructores

        public Country()
        {
        }

        public Country(string idCountry, string name, decimal taxBase)
        {
            IdCountry = idCountry;
            Name = name;
            TaxBase = taxBase;
        }

        #endregion

        #region Persistencia (a través de CountryDAO)

        /// <summary>DAO de los países: la entidad no accede directamente a la base de datos.</summary>
        private static readonly CountryDAO dao = new CountryDAO();

        /// <summary>Valida e inserta el país. Si no tiene código, lo genera a partir del nombre.</summary>
        public bool Insert()
        {
            if (string.IsNullOrWhiteSpace(IdCountry))
            {
                IdCountry = CountryCodeGenerator.GenerateCode(Name);
            }

            Validate();
            return dao.Insert(IdCountry.ToUpper().Trim(), Name.Trim(), TaxBase) > 0;
        }

        /// <summary>Valida y actualiza el país.</summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdCountry.ToUpper().Trim(), Name.Trim(), TaxBase) > 0;
        }

        /// <summary>Elimina el país si no tiene oficinas ni empleados asociados.</summary>
        public bool Delete()
        {
            dao.CountDependents(IdCountry);
            if (dao.Total > 0)
            {
                throw new InvalidOperationException("El país tiene oficinas o empleados asociados.");
            }

            return dao.Delete(IdCountry) > 0;
        }

        /// <summary>Todos los países, ordenados por nombre.</summary>
        public static List<Country> ReadAll()
        {
            dao.ReadAll();
            return dao.Countries;
        }

        /// <summary>País con ese código, o null si no existe.</summary>
        public static Country? Read(string idCountry)
        {
            dao.Read(idCountry);
            return dao.Countries.Count > 0 ? dao.Countries[0] : null;
        }

        /// <summary>Indica si ya existe un país con ese código.</summary>
        public static bool Exists(string idCountry)
        {
            dao.CountById(idCountry);
            return dao.Total > 0;
        }

        #endregion

        #region Validación

        /// <summary>Lanza una excepción si algún dato no es válido.</summary>
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
