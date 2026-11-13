namespace Company.persistence
{
    /// <summary>
    /// Comprueba la conexión con la base de datos abriéndola y cerrándola con AgenteBD.
    /// </summary>
    internal class ConnectionDAO
    {
        /// <summary>Lanza una excepción si no es posible conectar.</summary>
        public void TestConnection()
        {
            AgenteBD agente = AgenteBD.GetInstance();
            agente.connect();
            agente.disconnect();
        }
    }
}
