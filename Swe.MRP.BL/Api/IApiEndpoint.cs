using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Api;

public interface IApiEndpoint
{
    int Version { get; }
    string EndpointName { get; }
    void Behaviour(ServiceProvider services, Dictionary<string, string> args);
}
