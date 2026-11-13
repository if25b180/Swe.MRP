using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL;
using Swe.MRP.BL.Api;
using Swe.MRP.BL.Api.Endpoints;
using Swe.MRP.BL.Services;
using Swe.MRP.DL;

namespace Swe.MRP.App;

internal static class Program
{
    static string Env(string str)
    {
        return Environment.GetEnvironmentVariable(str) ?? "";
    }
    
    // Returns success = true / false
    static bool ReadEnvironment()
    {
        _ = DotNetEnv.Env.TraversePath().Load();
        if (Env("NPGSQL_CONNECTION_STRING") == null)
        {
            Console.WriteLine("Konnte connectionString nicht auslesen! Stelle sicher, dass 'NPGSQL_CONNECTION_STRING' in der .env-File gesetzt ist!");
            return false;
        }

        return true;
    }

    static async Task Main()
    {
        if (!ReadEnvironment())
        {
            Console.WriteLine("Nicht alle benötigten Environment Variablen konnten erfolgreich geladen werden!");
            Environment.Exit(-1);
        }

        ServiceCollection services = new();

        Database database = new(Env("NPGSQL_CONNECTION_STRING"));
        services.AddSingleton<Database>(_ => database);

        services.AddSingleton<Authentication>();
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<RegisterEndpoint>();

        services.AddSingleton<EndpointRegistrar>
            (provider => new EndpointRegistrar(provider));

        ServiceProvider provider = services.BuildServiceProvider();

        const string localhost = "http://localhost:8080/";
        MRPServer server = new(localhost, provider);
        server.Start();
        
        server.WaitForTermination();
        Console.WriteLine("Server terminiert.");
    }
}
