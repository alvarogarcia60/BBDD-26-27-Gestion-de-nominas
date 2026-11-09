using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Company.persistence
{
    internal class AgenteBD
    {
        private static string? connectionString;
        private static MySqlConnection? connection;
        private static AgenteBD? instance;

        private AgenteBD()
        {
            AgenteBD.connectionString = "server=localhost;database=Company;uid=root;pwd=password;CharSet=utf8;";
            AgenteBD.connection = new MySqlConnection(AgenteBD.connectionString);
        }
        public static AgenteBD GetInstance()
        {
            if (instance == null)
            {
                instance = new AgenteBD();
            }
            return instance;
        }
        public List<string[]> Read(string sql)
        {
            List<string[]> result = new List<string[]>();
            MySqlCommand command = new MySqlCommand(sql, connection);

            connect();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string[] row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i].ToString();
                }
                result.Add(row);
            }
            reader.Close();
            disconnect();
            return result;
        }
        public int Change(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql, connection);
            int result;
            connect();
            result = command.ExecuteNonQuery();
            disconnect();
            return result;
        }
        public void connect()
        {
            if (AgenteBD.connection != null)
            {
                if (AgenteBD.connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
        }
        public void disconnect()
        {
            if (AgenteBD.connection != null)
            {
                if (AgenteBD.connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}
