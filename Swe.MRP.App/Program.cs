using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL;
using Swe.MRP.BL.Api;
using Swe.MRP.BL.Api.Endpoints;
using Swe.MRP.BL.Services;

namespace Swe.MRP.App;

internal static class Program
{
    static void Main()
    {
        ServiceCollection services = new();
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<RegisterEndpoint>();
        ServiceProvider provider = services.BuildServiceProvider();

        EndpointRegistrar endpointRegistrar = new(provider);

        const string localhost = "http://localhost:8080/";
        MRPServer server = new(localhost, endpointRegistrar);
        server.Start();

        _ = DotNetEnv.Env.TraversePath().Load();
        string? connectionString = Environment.GetEnvironmentVariable("NPGSQL_CONNECTION_STRING");
        if (connectionString == null)
        {
            Console.WriteLine("Konnte nicht zur Datenbank verbinden! Stelle sicher, dass 'NPGSQL_CONNECTION_STRING' in der .env-File gesetzt ist!");
            return;
        }
        server.InitDatabase(connectionString);
        
        server.WaitForTermination();
        Console.WriteLine("Server terminiert.");
    }
}
