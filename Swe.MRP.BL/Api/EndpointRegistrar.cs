using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Api;

public class EndpointRegistrar
{
    private readonly Dictionary<string, IApiEndpoint> _endpoints = [];
    private readonly IServiceProvider _serviceProvider;
    
    public EndpointRegistrar(IServiceProvider serviceProvider)
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

    public void Handle(string endpointPath, Dictionary<string, string> args)
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

        endpoint?.Behaviour(_serviceProvider, args);
    }
}
