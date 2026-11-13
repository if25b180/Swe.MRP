using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Abstractions.Api;

public interface IApiEndpoint
{
    int Version { get; }
    string EndpointName { get; }
    void Behaviour(IServiceProvider services, Dictionary<string, string> args);
}
