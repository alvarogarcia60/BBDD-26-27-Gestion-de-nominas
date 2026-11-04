using System;
using System.Collections.Generic;
using System.Data;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: Employee (Empleado).
    /// Mapea la tabla 'Employees' del esquema relacional.
    /// Representa un trabajador de la organización multinacional.
    /// Cumple estrictamente con la POO: la clave foránea empCountry se modela como
    /// una referencia al objeto de dominio Country, no como un tipo primitivo.
    /// </summary>
    public class Employee
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Identificador único autonumérico del empleado (Clave Primaria, INT).
        /// </summary>
        public int IdEmployee { get; set; }

        /// <summary>
        /// Nombre y apellidos completos del empleado (VARCHAR(120)).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Relación de Dominio POO: País de residencia u origen del empleado.
        /// Corresponde a la clave foránea 'empCountry' vinculada a la tabla Countries.
        /// </summary>
        public Country Country { get; set; } = new Country();

        /// <summary>
        /// Código de cuenta bancaria internacional IBAN (VARCHAR(34), Clave Única).
        /// </summary>
        public string IBAN { get; set; } = string.Empty;

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para enlaces de datos, serialización y mapeo.
        /// </summary>
        public Employee()
        {
        }

        /// <summary>
        /// Constructor parametrizado para nuevos empleados (sin identificador de base de datos).
        /// </summary>
        /// <param name="name">Nombre completo del trabajador.</param>
        /// <param name="country">Objeto Country asociado.</param>
        /// <param name="iban">Código de cuenta bancaria IBAN.</param>
        public Employee(string name, Country country, string iban)
        {
            Name = name;
            Country = country ?? throw new ArgumentNullException(nameof(country), "El país del empleado no puede ser nulo.");
            IBAN = iban;
        }

        /// <summary>
        /// Constructor parametrizado completo (para reconstrucción desde la persistencia).
        /// </summary>
        /// <param name="idEmployee">Identificador del empleado.</param>
        /// <param name="name">Nombre completo del trabajador.</param>
        /// <param name="country">Objeto Country asociado.</param>
        /// <param name="iban">Código de cuenta bancaria IBAN.</param>
        public Employee(int idEmployee, string name, Country country, string iban)
            : this(name, country, iban)
        {
            IdEmployee = idEmployee;
        }

        #endregion

        #region Métodos de Persistencia (Patrón Database Broker)

        /// <summary>
        /// Inserta el empleado en la base de datos a través del Database Broker.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = @"INSERT INTO Employees (empName, empCountry, empIBAN) 
                           VALUES (@empName, @empCountry, @empIBAN);";

            var parameters = new Dictionary<string, object?>
            {
                { "@empName", Name.Trim() },
                { "@empCountry", Country.IdCountry },
                { "@empIBAN", IBAN.Trim().Replace(" ", "").ToUpper() }
            };

            long generatedId = DBBroker.GetInstance().ExecuteInsertAndGetId(sql, parameters);
            if (generatedId > 0)
            {
                IdEmployee = (int)generatedId;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actualiza los datos del empleado en la base de datos.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = @"UPDATE Employees 
                           SET empName = @empName, empCountry = @empCountry, empIBAN = @empIBAN 
                           WHERE idEmployee = @idEmployee;";

            var parameters = new Dictionary<string, object?>
            {
                { "@idEmployee", IdEmployee },
                { "@empName", Name.Trim() },
                { "@empCountry", Country.IdCountry },
                { "@empIBAN", IBAN.Trim().Replace(" ", "").ToUpper() }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Elimina el empleado de la base de datos.
        /// </summary>
        public bool Delete()
        {
            string sql = "DELETE FROM Employees WHERE idEmployee = @idEmployee;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idEmployee", IdEmployee }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Lee y mapea todos los empleados de la base de datos cargando su objeto Country.
        /// </summary>
        public static List<Employee> ReadAll()
        {
            string sql = @"SELECT e.idEmployee, e.empName, e.empCountry, e.empIBAN,
                                  c.couName, c.couTaxBase
                           FROM Employees e
                           INNER JOIN Countries c ON e.empCountry = c.idCountry
                           ORDER BY e.empName ASC;";

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql);
            var list = new List<Employee>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Obtiene un empleado por su ID primario cargando su objeto Country.
        /// </summary>
        public static Employee? Read(int idEmployee)
        {
            string sql = @"SELECT e.idEmployee, e.empName, e.empCountry, e.empIBAN,
                                  c.couName, c.couTaxBase
                           FROM Employees e
                           INNER JOIN Countries c ON e.empCountry = c.idCountry
                           WHERE e.idEmployee = @idEmployee;";

            var parameters = new Dictionary<string, object?>
            {
                { "@idEmployee", idEmployee }
            };

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapFromRow(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Mapea una fila de datos a un objeto Employee instanciando su propiedad Country.
        /// </summary>
        private static Employee MapFromRow(DataRow row)
        {
            var country = new Country
            {
                IdCountry = row["empCountry"].ToString() ?? string.Empty,
                Name = row.Table.Columns.Contains("couName") ? row["couName"].ToString() ?? string.Empty : string.Empty,
                TaxBase = row.Table.Columns.Contains("couTaxBase") ? Convert.ToDecimal(row["couTaxBase"]) : 0m
            };

            return new Employee
            {
                IdEmployee = Convert.ToInt32(row["idEmployee"]),
                Name = row["empName"].ToString() ?? string.Empty,
                Country = country,
                IBAN = row["empIBAN"].ToString() ?? string.Empty
            };
        }

        #endregion

        #region Métodos de Negocio (Lógica de Antigüedad y Trienios)

        /// <summary>
        /// Calcula el total de días trabajados por el empleado en cualquier puesto hasta una fecha límite.
        /// Considera únicamente periodos laborales hasta la fecha evaluada, ignorando fechas posteriores.
        /// </summary>
        /// <param name="upToDate">Fecha límite de cómputo (normalmente el último día del mes liquidado).</param>
        public int CalculateDaysWorked(DateTime upToDate)
        {
            var positions = Position.ReadByEmployee(IdEmployee);
            int totalDays = 0;

            foreach (var pos in positions)
            {
                if (pos.StartDate > upToDate)
                {
                    // Puesto comenzado con posterioridad a la fecha límite: no computa
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

        /// <summary>
        /// Calcula el número de trienios completos consolidados (cada 1095 días = 1 trienio)
        /// devengados hasta la fecha indicada.
        /// </summary>
        /// <param name="upToDate">Fecha límite de evaluación.</param>
        public int CalculateTrienios(DateTime upToDate)
        {
            int totalDays = CalculateDaysWorked(upToDate);
            return totalDays / 1095;
        }

        #endregion

        #region Reglas de Validación de Negocio

        /// <summary>
        /// Valida la consistencia de los datos del empleado.
        /// </summary>
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
