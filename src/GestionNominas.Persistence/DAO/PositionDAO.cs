using System;
using System.Collections.Generic;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// DAO de la tabla Positions. Contiene todo el SQL de los puestos y lo ejecuta con AgenteBD.
    /// Cada fila se devuelve con las columnas:
    ///  0 idPosition, 1 posEmployee, 2 posOffice, 3 posName, 4 posYearSalary, 5 posStartDate, 6 posEndDate,
    ///  7 empName, 8 empCountry, 9 empIBAN,
    ///  10 offCountry, 11 offCity, 12 offAddress, 13 offThreeYearBonus,
    ///  14-15 nombre e impuesto del país de la oficina, 16-17 nombre e impuesto del país del empleado.
    /// </summary>
    internal class PositionDAO
    {
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

        public List<string[]> ReadAll()
        {
            return AgenteBD.GetInstance().Read(SelectSql + " ORDER BY p.posStartDate DESC;");
        }

        public List<string[]> Read(int idPosition)
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} WHERE p.idPosition = {idPosition};");
        }

        public List<string[]> ReadByEmployee(int idEmployee)
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} WHERE p.posEmployee = {idEmployee} ORDER BY p.posStartDate ASC;");
        }

        public int Insert(int idEmployee, int idOffice, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
        {
            return AgenteBD.GetInstance().Change(
                $"INSERT INTO Positions (posEmployee, posOffice, posName, posYearSalary, posStartDate, posEndDate) " +
                $"VALUES ({idEmployee}, {idOffice}, {SqlText(name)}, {SqlNumber(yearSalary)}, " +
                $"{SqlDate(startDate)}, {SqlDateOrNull(endDate)});");
        }

        public int Update(int idPosition, int idEmployee, int idOffice, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Positions SET posEmployee = {idEmployee}, posOffice = {idOffice}, " +
                $"posName = {SqlText(name)}, posYearSalary = {SqlNumber(yearSalary)}, " +
                $"posStartDate = {SqlDate(startDate)}, posEndDate = {SqlDateOrNull(endDate)} " +
                $"WHERE idPosition = {idPosition};");
        }

        public int Delete(int idPosition)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Positions WHERE idPosition = {idPosition};");
        }
    }
}
