using System;
using System.Collections.Generic;
using GestionNominas.Domain.Entities;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Acceso a la tabla Positions: ejecuta el SQL con AgenteBD y construye los objetos Position.
    /// Las lecturas dejan el resultado en Positions.
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

        /// <summary>Puestos construidos en la última lectura.</summary>
        public List<Position> Positions { get; private set; } = new List<Position>();

        /// <summary>Lee todos los puestos, del más reciente al más antiguo.</summary>
        public void ReadAll()
        {
            Load(SelectSql + " ORDER BY p.posStartDate DESC;");
        }

        /// <summary>Lee el puesto con ese identificador (la lista queda vacía si no existe).</summary>
        public void Read(int idPosition)
        {
            Load($"{SelectSql} WHERE p.idPosition = {idPosition};");
        }

        /// <summary>Lee los puestos de un empleado, del más antiguo al más reciente.</summary>
        public void ReadByEmployee(int idEmployee)
        {
            Load($"{SelectSql} WHERE p.posEmployee = {idEmployee} ORDER BY p.posStartDate ASC;");
        }

        /// <summary>Inserta un puesto. Devuelve las filas afectadas.</summary>
        public int Insert(int idEmployee, int idOffice, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
        {
            return AgenteBD.GetInstance().Change(
                "INSERT INTO Positions (posEmployee, posOffice, posName, posYearSalary, posStartDate, posEndDate) " +
                $"VALUES ({idEmployee}, {idOffice}, {SqlText(name)}, {SqlNumber(yearSalary)}, " +
                $"{SqlDate(startDate)}, {SqlDateOrNull(endDate)});");
        }

        /// <summary>Actualiza un puesto. Devuelve las filas afectadas.</summary>
        public int Update(int idPosition, int idEmployee, int idOffice, string name, decimal yearSalary, DateTime startDate, DateTime? endDate)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Positions SET posEmployee = {idEmployee}, posOffice = {idOffice}, " +
                $"posName = {SqlText(name)}, posYearSalary = {SqlNumber(yearSalary)}, " +
                $"posStartDate = {SqlDate(startDate)}, posEndDate = {SqlDateOrNull(endDate)} " +
                $"WHERE idPosition = {idPosition};");
        }

        /// <summary>Elimina un puesto. Devuelve las filas afectadas.</summary>
        public int Delete(int idPosition)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Positions WHERE idPosition = {idPosition};");
        }

        /// <summary>Ejecuta el SELECT y convierte cada fila en un Position con su Employee y su Office.</summary>
        private void Load(string sql)
        {
            Positions = new List<Position>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
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

                Positions.Add(new Position
                {
                    IdPosition = ReadInt(row[0]),
                    Employee = employee,
                    Office = office,
                    Name = row[3],
                    YearSalary = ReadDecimal(row[4]),
                    StartDate = ReadDate(row[5]),
                    EndDate = ReadDateOrNull(row[6])
                });
            }
        }
    }
}
