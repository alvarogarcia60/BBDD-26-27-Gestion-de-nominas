using System;
using System.Collections.Generic;
using Company.persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Puesto de trabajo (tabla Positions): un empleado en una oficina, con un salario anual,
    /// entre una fecha de inicio y una de fin (sin fecha de fin si el puesto sigue activo).
    /// El empleado y la oficina se guardan como objetos Employee y Office.
    /// </summary>
    public class Position
    {
        #region Propiedades

        /// <summary>Clave primaria (autonumérica).</summary>
        public int IdPosition { get; set; }

        /// <summary>Empleado del puesto (clave foránea posEmployee).</summary>
        public Employee Employee { get; set; } = new Employee();

        /// <summary>Oficina del puesto (clave foránea posOffice).</summary>
        public Office Office { get; set; } = new Office();

        /// <summary>Nombre del cargo.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Salario anual bruto.</summary>
        public decimal YearSalary { get; set; }

        /// <summary>Fecha de inicio.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Fecha de fin; null si el puesto sigue activo.</summary>
        public DateTime? EndDate { get; set; }

        #endregion

        #region Constructores

        public Position()
        {
        }

        /// <summary>Puesto nuevo, todavía sin identificador.</summary>
        public Position(Employee employee, Office office, string name, decimal yearSalary, DateTime startDate, DateTime? endDate = null)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee), "El empleado no puede ser nulo.");
            Office = office ?? throw new ArgumentNullException(nameof(office), "La oficina no puede ser nula.");
            Name = name;
            YearSalary = yearSalary;
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>Puesto ya guardado, con su identificador.</summary>
        public Position(int idPosition, Employee employee, Office office, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
            : this(employee, office, name, yearSalary, startDate, endDate)
        {
            IdPosition = idPosition;
        }

        #endregion

        #region Persistencia (a través de PositionDAO)

        /// <summary>DAO de los puestos: la entidad no accede directamente a la base de datos.</summary>
        private static readonly PositionDAO dao = new PositionDAO();

        /// <summary>Valida e inserta el puesto.</summary>
        public bool Insert()
        {
            Validate();
            return dao.Insert(Employee.IdEmployee, Office.IdOffice, Name.Trim(), YearSalary, StartDate, EndDate) > 0;
        }

        /// <summary>Valida y actualiza el puesto.</summary>
        public bool Update()
        {
            Validate();
            return dao.Update(IdPosition, Employee.IdEmployee, Office.IdOffice, Name.Trim(), YearSalary, StartDate, EndDate) > 0;
        }

        /// <summary>Elimina el puesto.</summary>
        public bool Delete()
        {
            return dao.Delete(IdPosition) > 0;
        }

        /// <summary>Todos los puestos, con su empleado y su oficina.</summary>
        public static List<Position> ReadAll()
        {
            dao.ReadAll();
            return dao.Positions;
        }

        /// <summary>Puesto con ese identificador, o null si no existe.</summary>
        public static Position? Read(int idPosition)
        {
            dao.Read(idPosition);
            return dao.Positions.Count > 0 ? dao.Positions[0] : null;
        }

        /// <summary>Puestos de un empleado, del más antiguo al más reciente.</summary>
        public static List<Position> ReadByEmployee(int employeeId)
        {
            dao.ReadByEmployee(employeeId);
            return dao.Positions;
        }

        #endregion

        #region Validación

        /// <summary>
        /// Lanza una excepción si algún dato no es válido. La fecha de fin, si existe,
        /// debe ser anterior a hoy y el puesto no puede solaparse con otro del mismo empleado.
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

            if (HasDateOverlap(Employee.IdEmployee, StartDate, EndDate, IdPosition))
            {
                throw new InvalidOperationException("Conflicto de fechas: El empleado ya tiene otro puesto asignado en el rango temporal indicado.");
            }
        }

        /// <summary>
        /// Indica si las fechas se solapan con otro puesto del empleado (sin contar el propio puesto al editarlo).
        /// Dos periodos [A, B] y [C, D] se solapan si el mayor inicio es anterior o igual al menor fin.
        /// </summary>
        public static bool HasDateOverlap(int employeeId, DateTime newStart, DateTime? newEnd, int currentPositionId = 0)
        {
            var existingPositions = ReadByEmployee(employeeId);
            DateTime effectiveNewEnd = newEnd ?? DateTime.MaxValue;

            foreach (var pos in existingPositions)
            {
                if (pos.IdPosition == currentPositionId)
                {
                    continue;
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
