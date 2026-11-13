using System.Collections.Generic;
using GestionNominas.Domain.Entities;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Acceso a la tabla Countries: ejecuta el SQL con AgenteBD y construye los objetos Country.
    /// Las lecturas dejan el resultado en Countries o en Total.
    /// </summary>
    internal class CountryDAO
    {
        private const string SelectSql = "SELECT idCountry, couName, couTaxBase FROM Countries";

        /// <summary>Países construidos en la última lectura.</summary>
        public List<Country> Countries { get; private set; } = new List<Country>();

        /// <summary>Resultado del último recuento.</summary>
        public int Total { get; private set; }

        /// <summary>Lee todos los países, ordenados por nombre.</summary>
        public void ReadAll()
        {
            Load(SelectSql + " ORDER BY couName ASC;");
        }

        /// <summary>Lee el país con ese código (la lista queda vacía si no existe).</summary>
        public void Read(string idCountry)
        {
            Load($"{SelectSql} WHERE idCountry = {SqlText(idCountry)};");
        }

        /// <summary>Inserta un país. Devuelve las filas afectadas.</summary>
        public int Insert(string idCountry, string name, decimal taxBase)
        {
            return AgenteBD.GetInstance().Change(
                "INSERT INTO Countries (idCountry, couName, couTaxBase) " +
                $"VALUES ({SqlText(idCountry)}, {SqlText(name)}, {SqlNumber(taxBase)});");
        }

        /// <summary>Actualiza un país. Devuelve las filas afectadas.</summary>
        public int Update(string idCountry, string name, decimal taxBase)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Countries SET couName = {SqlText(name)}, couTaxBase = {SqlNumber(taxBase)} " +
                $"WHERE idCountry = {SqlText(idCountry)};");
        }

        /// <summary>Elimina un país. Devuelve las filas afectadas.</summary>
        public int Delete(string idCountry)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Countries WHERE idCountry = {SqlText(idCountry)};");
        }

        /// <summary>Cuenta los países con ese código (0 o 1).</summary>
        public void CountById(string idCountry)
        {
            Total = ReadInt(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM Countries WHERE idCountry = {SqlText(idCountry)};")[0][0]);
        }

        /// <summary>Cuenta las oficinas y empleados que pertenecen al país.</summary>
        public void CountDependents(string idCountry)
        {
            string id = SqlText(idCountry);
            Total = ReadInt(AgenteBD.GetInstance().Read(
                $"SELECT (SELECT COUNT(*) FROM Offices WHERE offCountry = {id}) + " +
                $"(SELECT COUNT(*) FROM Employees WHERE empCountry = {id});")[0][0]);
        }

        /// <summary>Ejecuta el SELECT y convierte cada fila en un Country.</summary>
        private void Load(string sql)
        {
            Countries = new List<Country>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                Countries.Add(new Country
                {
                    IdCountry = row[0],
                    Name = row[1],
                    TaxBase = ReadDecimal(row[2])
                });
            }
        }
    }
}
