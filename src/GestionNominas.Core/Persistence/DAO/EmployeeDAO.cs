using System.Collections.Generic;
using GestionNominas.Domain.Entities;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Acceso a la tabla Employees: ejecuta el SQL con AgenteBD y construye los objetos Employee.
    /// Las lecturas dejan el resultado en Employees o en Total.
    /// </summary>
    internal class EmployeeDAO
    {
        private const string SelectSql = @"SELECT e.idEmployee, e.empName, e.empCountry, e.empIBAN,
                                                  c.couName, c.couTaxBase
                                           FROM Employees e
                                           INNER JOIN Countries c ON e.empCountry = c.idCountry";

        /// <summary>Empleados construidos en la última lectura.</summary>
        public List<Employee> Employees { get; private set; } = new List<Employee>();

        /// <summary>Resultado del último recuento.</summary>
        public int Total { get; private set; }

        /// <summary>Lee todos los empleados, ordenados por nombre.</summary>
        public void ReadAll()
        {
            Load(SelectSql + " ORDER BY e.empName ASC;");
        }

        /// <summary>Lee el empleado con ese identificador (la lista queda vacía si no existe).</summary>
        public void Read(int idEmployee)
        {
            Load($"{SelectSql} WHERE e.idEmployee = {idEmployee};");
        }

        /// <summary>Inserta un empleado. Devuelve las filas afectadas.</summary>
        public int Insert(string name, string idCountry, string iban)
        {
            return AgenteBD.GetInstance().Change(
                "INSERT INTO Employees (empName, empCountry, empIBAN) " +
                $"VALUES ({SqlText(name)}, {SqlText(idCountry)}, {SqlText(iban)});");
        }

        /// <summary>Actualiza un empleado. Devuelve las filas afectadas.</summary>
        public int Update(int idEmployee, string name, string idCountry, string iban)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Employees SET empName = {SqlText(name)}, empCountry = {SqlText(idCountry)}, " +
                $"empIBAN = {SqlText(iban)} WHERE idEmployee = {idEmployee};");
        }

        /// <summary>Elimina un empleado. Devuelve las filas afectadas.</summary>
        public int Delete(int idEmployee)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Employees WHERE idEmployee = {idEmployee};");
        }

        /// <summary>Cuenta los puestos y nóminas del empleado.</summary>
        public void CountPositionsAndPayments(int idEmployee)
        {
            Total = ReadInt(AgenteBD.GetInstance().Read(
                $"SELECT (SELECT COUNT(*) FROM Positions WHERE posEmployee = {idEmployee}) + " +
                $"(SELECT COUNT(*) FROM MonthlyPayments WHERE payEmployee = {idEmployee});")[0][0]);
        }

        /// <summary>Ejecuta el SELECT y convierte cada fila en un Employee con su Country.</summary>
        private void Load(string sql)
        {
            Employees = new List<Employee>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                Employees.Add(new Employee
                {
                    IdEmployee = ReadInt(row[0]),
                    Name = row[1],
                    Country = new Country { IdCountry = row[2], Name = row[4], TaxBase = ReadDecimal(row[5]) },
                    IBAN = row[3]
                });
            }
        }
    }
}
