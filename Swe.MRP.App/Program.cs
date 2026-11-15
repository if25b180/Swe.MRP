using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Endpoints;
using Swe.MRP.DL;
using Swe.MRP.DL.Repository;

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

    static MRPServer StartServer(ServiceProvider provider)
    {
        const string localhost = "http://localhost:8080/";
        MRPServer server = new(localhost, provider);
        server.Start();

        return server;
    }

    static ServiceProvider RegisterServices()
    {
        ServiceCollection services = new();

        services.AddSingleton<Database>(_ => new(Env("NPGSQL_CONNECTION_STRING")));
        services.AddSingleton<EndpointHandler>();

        services.AddSingleton<IApiEndpoint, RegisterEndpoint>();
        services.AddSingleton<IApiEndpoint, LoginEndpoint>();
        services.AddSingleton<IApiEndpoint, MediaEntryCreate>();
        services.AddSingleton<IApiEndpoint, MediaEntryDelete>();
        services.AddSingleton<IApiEndpoint, MediaEntryGetById>();
        services.AddSingleton<IApiEndpoint, MediaEntryUpdate>();

        services.AddSingleton<IUserRepository, AdoNetUserRepository>();
        services.AddSingleton<IMediaEntryRepository, AdoNetMediaEntryRepository>();

        ServiceProvider provider = services.BuildServiceProvider();
        return provider;
    }

    static async Task Main()
    {
        if (!ReadEnvironment())
        {
            Console.WriteLine("Nicht alle benötigten Environment Variablen konnten erfolgreich geladen werden!");
            Environment.Exit(-1);
        }

        ServiceProvider provider = RegisterServices();

        Database database = provider.GetRequiredService<Database>();
        database.ApplyMissingSchemas();

        MRPServer server = StartServer(provider);
        server.WaitForTermination();
        
        Console.WriteLine("Server terminiert.");
    }
}
