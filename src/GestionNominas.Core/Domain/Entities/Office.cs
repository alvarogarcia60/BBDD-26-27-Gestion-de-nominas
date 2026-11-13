using System;
using System.Collections.Generic;
using Company.persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Oficina o sede de la empresa (tabla Offices).
    /// El país (offCountry) se guarda como un objeto Country, no como un código.
    /// </summary>
    public class Office
    {
        #region Propiedades

        /// <summary>Clave primaria (autonumérica).</summary>
        public int IdOffice { get; set; }

        /// <summary>País de la oficina (clave foránea offCountry).</summary>
        public Country Country { get; set; } = new Country();

        /// <summary>Ciudad.</summary>
        public string City { get; set; } = string.Empty;

        /// <summary>Dirección.</summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>Porcentaje de bonus por cada trienio completo.</summary>
        public decimal ThreeYearBonus { get; set; }

        #endregion

        #region Constructores

        public Office()
        {
        }

        /// <summary>Oficina nueva, todavía sin identificador.</summary>
        public Office(Country country, string city, string address, decimal threeYearBonus)
        {
            Country = country ?? throw new ArgumentNullException(nameof(country), "El país asociado no puede ser nulo.");
            City = city;
            Address = address;
            ThreeYearBonus = threeYearBonus;
        }

        /// <summary>Oficina ya guardada, con su identificador.</summary>
        public Office(int idOffice, Country country, string city, string address, decimal threeYearBonus)
            : this(country, city, address, threeYearBonus)
        {
            IdOffice = idOffice;
        }

        #endregion

        #region Persistencia (a través de OfficeDAO)

        /// <summary>DAO de las oficinas: la entidad no accede directamente a la base de datos.</summary>
        private static readonly OfficeDAO dao = new OfficeDAO();

        /// <summary>Valida e inserta la oficina.</summary>
        public bool Insert()
        {
            Validate();
            return dao.Insert(Country.IdCountry, City.Trim(), Address.Trim(), ThreeYearBonus) > 0;
        }

        /// <summary>Valida y actualiza la oficina.</summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdOffice, Country.IdCountry, City.Trim(), Address.Trim(), ThreeYearBonus) > 0;
        }

        /// <summary>Elimina la oficina si no tiene puestos asociados.</summary>
        public bool Delete()
        {
            dao.CountPositions(IdOffice);
            if (dao.Total > 0)
            {
                throw new InvalidOperationException("La oficina tiene puestos de trabajo asociados.");
            }

            return dao.Delete(IdOffice) > 0;
        }

        /// <summary>Todas las oficinas, con su país.</summary>
        public static List<Office> ReadAll()
        {
            dao.ReadAll();
            return dao.Offices;
        }

        /// <summary>Oficina con ese identificador, o null si no existe.</summary>
        public static Office? Read(int idOffice)
        {
            dao.Read(idOffice);
            return dao.Offices.Count > 0 ? dao.Offices[0] : null;
        }

        #endregion

        #region Validación

        /// <summary>Lanza una excepción si algún dato no es válido.</summary>
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
