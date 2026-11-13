using System.Collections.Generic;
using GestionNominas.Domain.Entities;
using static Company.persistence.SqlFormat;

namespace Company.persistence
{
    /// <summary>
    /// Acceso a la tabla Offices: ejecuta el SQL con AgenteBD y construye los objetos Office.
    /// Las lecturas dejan el resultado en Offices o en Total.
    /// </summary>
    internal class OfficeDAO
    {
        private const string SelectSql = @"SELECT o.idOffice, o.offCountry, o.offCity, o.offAddress, o.offThreeYearBonus,
                                                  c.couName, c.couTaxBase
                                           FROM Offices o
                                           INNER JOIN Countries c ON o.offCountry = c.idCountry";

        /// <summary>Oficinas construidas en la última lectura.</summary>
        public List<Office> Offices { get; private set; } = new List<Office>();

        /// <summary>Resultado del último recuento.</summary>
        public int Total { get; private set; }

        /// <summary>Lee todas las oficinas, ordenadas por país y ciudad.</summary>
        public void ReadAll()
        {
            Load(SelectSql + " ORDER BY c.couName ASC, o.offCity ASC;");
        }

        /// <summary>Lee la oficina con ese identificador (la lista queda vacía si no existe).</summary>
        public void Read(int idOffice)
        {
            Load($"{SelectSql} WHERE o.idOffice = {idOffice};");
        }

        /// <summary>Inserta una oficina. Devuelve las filas afectadas.</summary>
        public int Insert(string idCountry, string city, string address, decimal threeYearBonus)
        {
            return AgenteBD.GetInstance().Change(
                "INSERT INTO Offices (offCountry, offCity, offAddress, offThreeYearBonus) " +
                $"VALUES ({SqlText(idCountry)}, {SqlText(city)}, {SqlText(address)}, {SqlNumber(threeYearBonus)});");
        }

        /// <summary>Actualiza una oficina. Devuelve las filas afectadas.</summary>
        public int Update(int idOffice, string idCountry, string city, string address, decimal threeYearBonus)
        {
            return AgenteBD.GetInstance().Change(
                $"UPDATE Offices SET offCountry = {SqlText(idCountry)}, offCity = {SqlText(city)}, " +
                $"offAddress = {SqlText(address)}, offThreeYearBonus = {SqlNumber(threeYearBonus)} " +
                $"WHERE idOffice = {idOffice};");
        }

        /// <summary>Elimina una oficina. Devuelve las filas afectadas.</summary>
        public int Delete(int idOffice)
        {
            return AgenteBD.GetInstance().Change($"DELETE FROM Offices WHERE idOffice = {idOffice};");
        }

        /// <summary>Cuenta los puestos de trabajo de la oficina.</summary>
        public void CountPositions(int idOffice)
        {
            Total = ReadInt(AgenteBD.GetInstance().Read(
                $"SELECT COUNT(*) FROM Positions WHERE posOffice = {idOffice};")[0][0]);
        }

        /// <summary>Ejecuta el SELECT y convierte cada fila en una Office con su Country.</summary>
        private void Load(string sql)
        {
            Offices = new List<Office>();
            foreach (string[] row in AgenteBD.GetInstance().Read(sql))
            {
                Offices.Add(new Office
                {
                    IdOffice = ReadInt(row[0]),
                    Country = new Country { IdCountry = row[1], Name = row[5], TaxBase = ReadDecimal(row[6]) },
                    City = row[2],
                    Address = row[3],
                    ThreeYearBonus = ReadDecimal(row[4])
                });
            }
        }
    }
}
