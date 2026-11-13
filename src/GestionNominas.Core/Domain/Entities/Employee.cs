using System;
using System.Collections.Generic;
using Company.persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Empleado de la empresa (tabla Employees).
    /// El país (empCountry) se guarda como un objeto Country, no como un código.
    /// </summary>
    public class Employee
    {
        #region Propiedades

        /// <summary>Clave primaria (autonumérica).</summary>
        public int IdEmployee { get; set; }

        /// <summary>Nombre y apellidos.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>País del empleado (clave foránea empCountry).</summary>
        public Country Country { get; set; } = new Country();

        /// <summary>Cuenta bancaria IBAN.</summary>
        public string IBAN { get; set; } = string.Empty;

        #endregion

        #region Constructores

        public Employee()
        {
        }

        /// <summary>Empleado nuevo, todavía sin identificador.</summary>
        public Employee(string name, Country country, string iban)
        {
            Name = name;
            Country = country ?? throw new ArgumentNullException(nameof(country), "El país del empleado no puede ser nulo.");
            IBAN = iban;
        }

        /// <summary>Empleado ya guardado, con su identificador.</summary>
        public Employee(int idEmployee, string name, Country country, string iban)
            : this(name, country, iban)
        {
            IdEmployee = idEmployee;
        }

        #endregion

        #region Persistencia (a través de EmployeeDAO)

        /// <summary>DAO de los empleados: la entidad no accede directamente a la base de datos.</summary>
        private static readonly EmployeeDAO dao = new EmployeeDAO();

        /// <summary>Valida e inserta el empleado.</summary>
        public bool Insert()
        {
            Validate();
            return dao.Insert(Name.Trim(), Country.IdCountry, NormalizedIban) > 0;
        }

        /// <summary>Valida y actualiza el empleado.</summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdEmployee, Name.Trim(), Country.IdCountry, NormalizedIban) > 0;
        }

        private string NormalizedIban => IBAN.Trim().Replace(" ", "").ToUpper();

        /// <summary>Elimina el empleado si no tiene puestos ni nóminas asociados.</summary>
        public bool Delete()
        {
            dao.CountPositionsAndPayments(IdEmployee);
            if (dao.Total > 0)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el empleado porque tiene puestos de trabajo o nóminas asociados.");
            }

            return dao.Delete(IdEmployee) > 0;
        }

        /// <summary>Todos los empleados, con su país.</summary>
        public static List<Employee> ReadAll()
        {
            dao.ReadAll();
            return dao.Employees;
        }

        /// <summary>Empleado con ese identificador, o null si no existe.</summary>
        public static Employee? Read(int idEmployee)
        {
            dao.Read(idEmployee);
            return dao.Employees.Count > 0 ? dao.Employees[0] : null;
        }

        #endregion

        #region Antigüedad y trienios

        /// <summary>Días trabajados en cualquier puesto hasta la fecha indicada (incluida).</summary>
        public int CalculateDaysWorked(DateTime upToDate)
        {
            var positions = Position.ReadByEmployee(IdEmployee);
            int totalDays = 0;

            foreach (var pos in positions)
            {
                if (pos.StartDate > upToDate)
                {
                    continue;
                }

                DateTime effectiveEnd = pos.EndDate.HasValue && pos.EndDate.Value < upToDate
                    ? pos.EndDate.Value
                    : upToDate;

                if (effectiveEnd >= pos.StartDate)
                {
                    totalDays += (effectiveEnd - pos.StartDate).Days + 1;
                }
            }

            return totalDays;
        }

        /// <summary>Trienios completos (cada 1095 días trabajados) hasta la fecha indicada.</summary>
        public int CalculateTrienios(DateTime upToDate)
        {
            int totalDays = CalculateDaysWorked(upToDate);
            return totalDays / 1095;
        }

        #endregion

        #region Validación

        /// <summary>Lanza una excepción si algún dato no es válido.</summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("El nombre del empleado no puede estar vacío.");
            }

            if (Country == null || string.IsNullOrWhiteSpace(Country.IdCountry))
            {
                throw new ArgumentException("El empleado debe tener asignado un país válido.");
            }

            if (string.IsNullOrWhiteSpace(IBAN) || IBAN.Trim().Length < 15 || IBAN.Trim().Length > 34)
            {
                throw new ArgumentException("El código IBAN no es válido (debe contener entre 15 y 34 caracteres).");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{Name} (ID: {IdEmployee})";
        }
    }
}
