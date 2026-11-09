using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

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
        /// Código de cuenta bancaria internacional IBAN (VARCHAR(34)).
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

        #region Métodos de Persistencia (AgenteBD)

        private const string SelectSql = @"SELECT e.idEmployee, e.empName, e.empCountry, e.empIBAN,
                                                  c.couName, c.couTaxBase
                                           FROM Employees e
                                           INNER JOIN Countries c ON e.empCountry = c.idCountry";

        /// <summary>
        /// Inserta el empleado.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = $"INSERT INTO Employees (empName, empCountry, empIBAN) " +
                         $"VALUES ({SqlText(Name.Trim())}, {SqlText(Country.IdCountry)}, {SqlText(NormalizedIban)});";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Actualiza los datos del empleado.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = $"UPDATE Employees SET empName = {SqlText(Name.Trim())}, empCountry = {SqlText(Country.IdCountry)}, " +
                         $"empIBAN = {SqlText(NormalizedIban)} WHERE idEmployee = {IdEmployee};";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        private string NormalizedIban => IBAN.Trim().Replace(" ", "").ToUpper();

        /// <summary>
        /// Elimina el empleado junto con sus nóminas y puestos.
        /// Las claves foráneas del esquema son NO ACTION, así que hay que borrar antes las filas hijas.
        /// </summary>
        public bool Delete()
        {
            AgenteBD agente = AgenteBD.GetInstance();
            agente.Change($"DELETE FROM MonthlyPayments WHERE payEmployee = {IdEmployee};");
            agente.Change($"DELETE FROM Positions WHERE posEmployee = {IdEmployee};");
            return agente.Change($"DELETE FROM Employees WHERE idEmployee = {IdEmployee};") > 0;
        }

        /// <summary>
        /// Lee todos los empleados cargando su objeto Country.
        /// </summary>
        public static List<Employee> ReadAll()
        {
            var list = new List<Employee>();
            foreach (string[] row in AgenteBD.GetInstance().Read(SelectSql + " ORDER BY e.empName ASC;"))
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Obtiene un empleado por su identificador.
        /// </summary>
        public static Employee? Read(int idEmployee)
        {
            List<string[]> rows = AgenteBD.GetInstance().Read($"{SelectSql} WHERE e.idEmployee = {idEmployee};");
            return rows.Count > 0 ? MapFromRow(rows[0]) : null;
        }

        private static Employee MapFromRow(string[] row)
        {
            return new Employee
            {
                IdEmployee = ReadInt(row[0]),
                Name = row[1],
                Country = new Country { IdCountry = row[2], Name = row[4], TaxBase = ReadDecimal(row[5]) },
                IBAN = row[3]
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
