using Swe.MRP.BL;

namespace Swe.MRP.App;

internal static class Program
{
    static void Main()
    {
        const string localhost = "http://localhost:8080/";
        MRPServer server = new(localhost);
        server.Start();

        _ = DotNetEnv.Env.TraversePath().Load();
        string? connectionString = Environment.GetEnvironmentVariable("NPGSQL_CONNECTION_STRING");
        if (connectionString == null)
        {
            Console.WriteLine("Konnte nicht zur Datenbank verbinden! Stelle sicher, dass 'POSTGRES_CONNECTION_STRING' in der .env-File gesetzt ist!");
            return;
        }
        server.InitDatabase(connectionString);
        
        server.WaitForTermination();
        Console.WriteLine("Server terminiert.");
    }
}
