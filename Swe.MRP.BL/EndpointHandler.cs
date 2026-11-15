using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;

namespace Swe.MRP.BL;

public class EndpointHandler
{
    public Dictionary<string, IApiEndpoint> Endpoints { get; } = [];

    public EndpointHandler(IServiceProvider provider)
    {   
        foreach (IApiEndpoint endpoint in provider.GetServices<IApiEndpoint>())
        {
            // We omit version for now, since the provided spec files do not expect it
            //Endpoints[$"/api/v{endpoint.Version}{endpoint.EndpointName}"] = endpoint;

            Endpoints[$"{endpoint.EndpointMethod} /api{endpoint.EndpointName}"] = endpoint;
        }
    }

    public void Call(string endpointPath, JsonElement urlParams, JsonElement bodyParams)
    {
        Console.WriteLine(endpointPath);

        if (!endpointPath.Contains("api/"))
        {
            // No particular api endpoint was called
            return;
        }

        if (!Endpoints.TryGetValue(endpointPath, out IApiEndpoint? endpoint))
        {
            Console.WriteLine($"Endpoint {endpointPath} existiert nicht");
            return;
        }

        endpoint?.Behaviour(urlParams, bodyParams);
    }
}
