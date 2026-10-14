using SWE.MRP.BL;

namespace Swe.MRP.App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string localhost = "http://localhost:8080/";
            MRPServer server = new MRPServer(localhost);
            server.start();
            
            DotNetEnv.Env.TraversePath().Load();
            string? connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
            if (connectionString == null)
            {
                Console.WriteLine("Konnte nicht zur Datenbank verbinden! Stelle sicher, dass 'POSTGRES_CONNECTION_STRING' in der .env-File gesetzt ist!");
                return;
            }

            server.connectDatabase(connectionString);
        }
    }
}
