using System;
using System.Collections.Generic;
using Company.persistence;
using static GestionNominas.Domain.Services.SqlFormat;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: Position (Puesto / Asignación Laboral).
    /// Mapea la tabla 'Positions' del esquema relacional.
    /// Vincula un empleado con una oficina, cargo y salario anual en un intervalo de tiempo.
    /// Aplica estrictamente los principios de POO: las claves foráneas posEmployee y posOffice
    /// se instancian como referencias directas a objetos de dominio Employee y Office.
    /// </summary>
    public class Position
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Identificador único autonumérico del puesto (Clave Primaria, INT).
        /// </summary>
        public int IdPosition { get; set; }

        /// <summary>
        /// Relación de Dominio POO: Empleado que desempeña o desempeñó el puesto.
        /// Corresponde a la clave foránea 'posEmployee' vinculada a la tabla Employees.
        /// </summary>
        public Employee Employee { get; set; } = new Employee();

        /// <summary>
        /// Relación de Dominio POO: Oficina física en la que se desempeña el puesto.
        /// Corresponde a la clave foránea 'posOffice' vinculada a la tabla Offices.
        /// </summary>
        public Office Office { get; set; } = new Office();

        /// <summary>
        /// Denominación o cargo profesional del puesto (VARCHAR(100)).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Salario anual bruto estipulado para el puesto (DECIMAL(11,2)).
        /// </summary>
        public decimal YearSalary { get; set; }

        /// <summary>
        /// Fecha de inicio en el puesto (DATE).
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Fecha de finalización en el puesto (DATE, NULL si el puesto está actualmente activo).
        /// Si tiene valor, debe ser en el pasado y no solaparse con otros puestos del empleado.
        /// </summary>
        public DateTime? EndDate { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para enlaces de datos, serialización y mapeo.
        /// </summary>
        public Position()
        {
        }

        /// <summary>
        /// Constructor parametrizado para nuevos puestos (sin identificador de base de datos).
        /// </summary>
        /// <param name="employee">Objeto Employee titular del puesto.</param>
        /// <param name="office">Objeto Office donde se desempeña.</param>
        /// <param name="name">Nombre o cargo del puesto.</param>
        /// <param name="yearSalary">Salario bruto anual.</param>
        /// <param name="startDate">Fecha de inicio.</param>
        /// <param name="endDate">Fecha de fin (opcional, null si está activo).</param>
        public Position(Employee employee, Office office, string name, decimal yearSalary, DateTime startDate, DateTime? endDate = null)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee), "El empleado no puede ser nulo.");
            Office = office ?? throw new ArgumentNullException(nameof(office), "La oficina no puede ser nula.");
            Name = name;
            YearSalary = yearSalary;
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        /// Constructor parametrizado completo (para reconstrucción desde la persistencia).
        /// </summary>
        /// <param name="idPosition">Identificador del puesto.</param>
        /// <param name="employee">Objeto Employee titular del puesto.</param>
        /// <param name="office">Objeto Office donde se desempeña.</param>
        /// <param name="name">Nombre o cargo del puesto.</param>
        /// <param name="yearSalary">Salario bruto anual.</param>
        /// <param name="startDate">Fecha de inicio.</param>
        /// <param name="endDate">Fecha de fin.</param>
        public Position(int idPosition, Employee employee, Office office, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
            : this(employee, office, name, yearSalary, startDate, endDate)
        {
            IdPosition = idPosition;
        }

        #endregion

        #region Métodos de Persistencia (AgenteBD)

        private const string SelectSql = @"SELECT p.idPosition, p.posEmployee, p.posOffice, p.posName, p.posYearSalary,
                                                  p.posStartDate, p.posEndDate,
                                                  e.empName, e.empCountry, e.empIBAN,
                                                  o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                                  cOffice.couName, cOffice.couTaxBase,
                                                  cEmp.couName, cEmp.couTaxBase
                                           FROM Positions p
                                           INNER JOIN Employees e ON p.posEmployee = e.idEmployee
                                           INNER JOIN Offices o ON p.posOffice = o.idOffice
                                           INNER JOIN Countries cOffice ON o.offCountry = cOffice.idCountry
                                           INNER JOIN Countries cEmp ON e.empCountry = cEmp.idCountry";

        /// <summary>
        /// Inserta el puesto previa validación de reglas de negocio y solapamiento.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = $"INSERT INTO Positions (posEmployee, posOffice, posName, posYearSalary, posStartDate, posEndDate) " +
                         $"VALUES ({Employee.IdEmployee}, {Office.IdOffice}, {SqlText(Name.Trim())}, {SqlNumber(YearSalary)}, " +
                         $"{SqlDate(StartDate)}, {SqlDateOrNull(EndDate)});";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Actualiza el puesto previa validación.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = $"UPDATE Positions SET posEmployee = {Employee.IdEmployee}, posOffice = {Office.IdOffice}, " +
                         $"posName = {SqlText(Name.Trim())}, posYearSalary = {SqlNumber(YearSalary)}, " +
                         $"posStartDate = {SqlDate(StartDate)}, posEndDate = {SqlDateOrNull(EndDate)} " +
                         $"WHERE idPosition = {IdPosition};";
            return AgenteBD.GetInstance().Change(sql) > 0;
        }

        /// <summary>
        /// Elimina el puesto.
        /// </summary>
        public bool Delete()
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Positions WHERE idPosition = {IdPosition};") > 0;
        }

        /// <summary>
        /// Lee todos los puestos con sus empleados y oficinas.
        /// </summary>
        public static List<Position> ReadAll()
        {
            return ReadList(SelectSql + " ORDER BY p.posStartDate DESC;");
        }

        /// <summary>
        /// Busca un puesto por su identificador.
        /// </summary>
        public static Position? Read(int idPosition)
        {
            List<Position> list = ReadList($"{SelectSql} WHERE p.idPosition = {idPosition};");
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Lee todos los puestos asignados a un empleado.
        /// </summary>
        public static List<Position> ReadByEmployee(int employeeId)
        {
            return ReadList($"{SelectSql} WHERE p.posEmployee = {employeeId} ORDER BY p.posStartDate ASC;");
        }

        private static List<Position> ReadList(string sql)
        {
            var list = new List<Position>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        private static Position MapFromRow(string[] row)
        {
            var employee = new Employee
            {
                IdEmployee = ReadInt(row[1]),
                Name = row[7],
                Country = new Country { IdCountry = row[8], Name = row[16], TaxBase = ReadDecimal(row[17]) },
                IBAN = row[9]
            };

            var office = new Office
            {
                IdOffice = ReadInt(row[2]),
                Country = new Country { IdCountry = row[10], Name = row[14], TaxBase = ReadDecimal(row[15]) },
                City = row[11],
                Address = row[12],
                ThreeYearBonus = ReadDecimal(row[13])
            };

            return new Position
            {
                IdPosition = ReadInt(row[0]),
                Employee = employee,
                Office = office,
                Name = row[3],
                YearSalary = ReadDecimal(row[4]),
                StartDate = ReadDate(row[5]),
                EndDate = ReadDateOrNull(row[6])
            };
        }

        #endregion

        #region Reglas de Negocio y Validaciones

        /// <summary>
        /// Valida la consistencia de los datos del puesto y comprueba que no existan solapamientos temporales:
        /// 1. Si posEndDate tiene valor, debe ser en el pasado o presente (posEndDate <= DateTime.Today).
        /// 2. posEndDate >= posStartDate.
        /// 3. No puede existir solapamiento con ningún otro puesto del mismo empleado.
        /// </summary>
        public void Validate()
        {
            if (Employee == null || Employee.IdEmployee <= 0)
            {
                throw new ArgumentException("Debe seleccionar un empleado válido.");
            }

            if (Office == null || Office.IdOffice <= 0)
            {
                throw new ArgumentException("Debe seleccionar una oficina válida.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("El nombre del puesto no puede estar vacío.");
            }

            if (YearSalary <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(YearSalary), "El salario anual debe ser un valor positivo.");
            }

            if (EndDate.HasValue)
            {
                if (EndDate.Value < StartDate)
                {
                    throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
                }

                if (EndDate.Value >= DateTime.Today)
                {
                    throw new ArgumentException("Si la fecha de fin tiene valor, debe ser una fecha en el pasado (anterior a hoy).");
                }
            }

            // Comprobación de solapamiento
            if (HasDateOverlap(Employee.IdEmployee, StartDate, EndDate, IdPosition))
            {
                throw new InvalidOperationException("Conflicto de fechas: El empleado ya tiene otro puesto asignado en el rango temporal indicado.");
            }
        }

        /// <summary>
        /// Comprueba si las fechas de un puesto se solapan con los demás puestos del empleado.
        /// Dos intervalos [A, B] y [C, D] se solapan si MAX(A, C) <= MIN(B, D).
        /// </summary>
        public static bool HasDateOverlap(int employeeId, DateTime newStart, DateTime? newEnd, int currentPositionId = 0)
        {
            var existingPositions = ReadByEmployee(employeeId);
            DateTime effectiveNewEnd = newEnd ?? DateTime.MaxValue;

            foreach (var pos in existingPositions)
            {
                if (pos.IdPosition == currentPositionId)
                {
                    continue; // Ignorar el mismo registro en caso de edición
                }

                DateTime existingStart = pos.StartDate;
                DateTime existingEnd = pos.EndDate ?? DateTime.MaxValue;

                DateTime overlapStart = newStart > existingStart ? newStart : existingStart;
                DateTime overlapEnd = effectiveNewEnd < existingEnd ? effectiveNewEnd : existingEnd;

                if (overlapStart <= overlapEnd)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calcula cuántos días trabajó el empleado en este puesto durante un mes y año específicos.
        /// </summary>
        public int CalculateDaysWorkedInMonth(int year, int month)
        {
            DateTime monthStart = new DateTime(year, month, 1);
            DateTime monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            DateTime effectivePosEnd = EndDate ?? DateTime.MaxValue;

            DateTime overlapStart = StartDate > monthStart ? StartDate : monthStart;
            DateTime overlapEnd = effectivePosEnd < monthEnd ? effectivePosEnd : monthEnd;

            if (overlapStart <= overlapEnd)
            {
                return (overlapEnd - overlapStart).Days + 1;
            }

            return 0;
        }

        #endregion

        public override string ToString()
        {
            string periodo = EndDate.HasValue 
                ? $"{StartDate:dd/MM/yyyy} a {EndDate.Value:dd/MM/yyyy}" 
                : $"Desde {StartDate:dd/MM/yyyy} (Activo)";
            return $"{Name} - {Employee.Name} ({periodo})";
        }
    }
}
