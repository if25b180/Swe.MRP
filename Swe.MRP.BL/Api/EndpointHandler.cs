using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;

namespace Swe.MRP.BL.Api;

public class EndpointHandler
{
    private readonly Dictionary<string, IApiEndpoint> _endpoints = [];

    public EndpointHandler(IServiceProvider provider)
    {   
        foreach (IApiEndpoint endpoint in provider.GetServices<IApiEndpoint>())
        {
            // We omit version for now, since the provided spec files do not expect it
            //_endpoints[$"/api/v{endpoint.Version}{endpoint.EndpointName}"] = endpoint;

            _endpoints[$"/api{endpoint.EndpointName}"] = endpoint;
        }
    }

    public void Call(string endpointPath, Dictionary<string, string> args)
    {
        Console.WriteLine(endpointPath);

        if (!endpointPath.Contains("api/"))
        {
            // No particular api endpoint was called
            return;
        }

        if (!_endpoints.TryGetValue(endpointPath, out IApiEndpoint? endpoint))
        {
            Console.WriteLine($"Endpoint {endpointPath} existiert nicht");
            return;
        }

        endpoint?.Behaviour(args);
    }
}
