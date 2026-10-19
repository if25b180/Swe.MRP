using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Api;

public class EndpointRegistrar
{
    private readonly Dictionary<string, IApiEndpoint> _endpoints = [];
    private readonly ServiceProvider _serviceProvider;
    
    public EndpointRegistrar(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Type apiEndpointType = typeof(IApiEndpoint);
        IEnumerable<Type> apiEndpointTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => apiEndpointType.IsAssignableFrom(type)
                && type.IsClass && !type.IsAbstract);
        
        foreach (Type endpointType in apiEndpointTypes)
        {
            IApiEndpoint endpoint = (IApiEndpoint) _serviceProvider.GetRequiredService(endpointType);
            _endpoints.Add($"/api/v{endpoint.Version}{endpoint.EndpointName}", endpoint);
        }
    }

    public void Handle(string endpointPath)
    {
        Console.WriteLine(endpointPath);

        // TODO: implement args parsing
        Dictionary<string, string> args = [];

        int paramIndex = endpointPath.IndexOf("api/");
        if (paramIndex == -1)
        {
            // No particular api endpoint was called
            return;
        }

        if (!_endpoints.ContainsKey(endpointPath))
        {
            Console.WriteLine($"Endpoint {endpointPath} existiert nicht");
            return;
        }

        _endpoints[endpointPath].Behaviour(_serviceProvider, args);
    }
}
