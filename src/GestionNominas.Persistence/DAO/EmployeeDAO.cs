using System.Collections.Generic;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// DAO de la tabla Employees. Contiene todo el SQL de los empleados y lo ejecuta con AgenteBD.
    /// Cada fila se devuelve con las columnas: idEmployee, empName, empCountry, empIBAN,
    /// couName, couTaxBase (país del empleado).
    /// </summary>
    internal class EmployeeDAO
    {
        private const string SelectSql = @"SELECT e.idEmployee, e.empName, e.empCountry, e.empIBAN,
                                                  c.couName, c.couTaxBase
                                           FROM Employees e
                                           INNER JOIN Countries c ON e.empCountry = c.idCountry";

        public List<string[]> ReadAll()
        {
            return AgenteBD.GetInstance().Read(SelectSql + " ORDER BY e.empName ASC;");
        }

        public List<string[]> Read(int idEmployee)
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} WHERE e.idEmployee = {idEmployee};");
        }

        public int Insert(string name, string idCountry, string iban)
        {
            return AgenteBD.GetInstance().Change(
                $"INSERT INTO Employees (empName, empCountry, empIBAN) " +
                $"VALUES ({SqlText(name)}, {SqlText(idCountry)}, {SqlText(iban)});");
        }

        public int Update(int idEmployee, string name, string idCountry, string iban)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Employees SET empName = {SqlText(name)}, empCountry = {SqlText(idCountry)}, " +
                $"empIBAN = {SqlText(iban)} WHERE idEmployee = {idEmployee};");
        }

        public int Delete(int idEmployee)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Employees WHERE idEmployee = {idEmployee};");
        }

        /// <summary>Número de puestos y nóminas del empleado.</summary>
        public int CountPositionsAndPayments(int idEmployee)
        {
            return ReadCount(AgenteBD.GetInstance().Read(
                $"SELECT (SELECT COUNT(*) FROM Positions WHERE posEmployee = {idEmployee}) + " +
                $"(SELECT COUNT(*) FROM MonthlyPayments WHERE payEmployee = {idEmployee});")[0][0]);
        }
    }
}
