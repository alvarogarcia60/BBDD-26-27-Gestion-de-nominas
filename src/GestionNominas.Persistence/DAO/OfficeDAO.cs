using System.Collections.Generic;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// DAO de la tabla Offices. Contiene todo el SQL de las oficinas y lo ejecuta con AgenteBD.
    /// Cada fila se devuelve con las columnas: idOffice, offCountry, offCity, offAddress,
    /// offThreeYearBonus, couName, couTaxBase (país de la oficina).
    /// </summary>
    internal class OfficeDAO
    {
        private const string SelectSql = @"SELECT o.idOffice, o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                                  c.couName, c.couTaxBase
                                           FROM Offices o
                                           INNER JOIN Countries c ON o.offCountry = c.idCountry";

        public List<string[]> ReadAll()
        {
            return AgenteBD.GetInstance().Read(SelectSql + " ORDER BY c.couName ASC, o.offCity ASC;");
        }

        public List<string[]> Read(int idOffice)
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} WHERE o.idOffice = {idOffice};");
        }

        public int Insert(string idCountry, string city, string address, decimal threeYearBonus)
        {
            return AgenteBD.GetInstance().Change(
                $"INSERT INTO Offices (offCountry, offCity, offAddress, offThreeYearBonus) " +
                $"VALUES ({SqlText(idCountry)}, {SqlText(city)}, {SqlText(address)}, {SqlNumber(threeYearBonus)});");
        }

        public int Update(int idOffice, string idCountry, string city, string address, decimal threeYearBonus)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Offices SET offCountry = {SqlText(idCountry)}, offCity = {SqlText(city)}, " +
                $"offAddress = {SqlText(address)}, offThreeYearBonus = {SqlNumber(threeYearBonus)} " +
                $"WHERE idOffice = {idOffice};");
        }

        public int Delete(int idOffice)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Offices WHERE idOffice = {idOffice};");
        }

        /// <summary>Número de puestos de trabajo de la oficina.</summary>
        public int CountPositions(int idOffice)
        {
            return ReadCount(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM Positions WHERE posOffice = {idOffice};")[0][0]);
        }
    }
}
