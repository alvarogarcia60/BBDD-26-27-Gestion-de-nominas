using System.Collections.Generic;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// DAO de la tabla Countries. Contiene todo el SQL de los países y lo ejecuta con AgenteBD.
    /// Cada fila se devuelve con las columnas: idCountry, couName, couTaxBase.
    /// </summary>
    internal class CountryDAO
    {
        private const string SelectSql = "SELECT idCountry, couName, couTaxBase FROM Countries";

        public List<string[]> ReadAll()
        {
            return AgenteBD.GetInstance().Read(SelectSql + " ORDER BY couName ASC;");
        }

        public List<string[]> Read(string idCountry)
        {
            return AgenteBD.GetInstance().Read($"{SelectSql} WHERE idCountry = {SqlText(idCountry)};");
        }

        public int Insert(string idCountry, string name, decimal taxBase)
        {
            return AgenteBD.GetInstance().Change(
                $"INSERT INTO Countries (idCountry, couName, couTaxBase) " +
                $"VALUES ({SqlText(idCountry)}, {SqlText(name)}, {SqlNumber(taxBase)});");
        }

        public int Update(string idCountry, string name, decimal taxBase)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Countries SET couName = {SqlText(name)}, couTaxBase = {SqlNumber(taxBase)} " +
                $"WHERE idCountry = {SqlText(idCountry)};");
        }

        public int Delete(string idCountry)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Countries WHERE idCountry = {SqlText(idCountry)};");
        }

        /// <summary>Número de países con ese código (0 o 1).</summary>
        public int Count(string idCountry)
        {
            return ReadCount(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM Countries WHERE idCountry = {SqlText(idCountry)};")[0][0]);
        }

        /// <summary>Número de oficinas y empleados que pertenecen al país.</summary>
        public int CountDependents(string idCountry)
        {
            string id = SqlText(idCountry);
            return ReadCount(AgenteBD.GetInstance().Read(
                $"SELECT (SELECT COUNT(*) FROM Offices WHERE offCountry = {id}) + " +
                $"(SELECT COUNT(*) FROM Employees WHERE empCountry = {id});")[0][0]);
        }
    }
}
