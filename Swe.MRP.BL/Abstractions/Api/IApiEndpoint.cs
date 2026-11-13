using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Abstractions.Api;

public interface IApiEndpoint
{
    IServiceProvider Services { get; }
    int Version { get; }
    string EndpointName { get; }
    void Behaviour(Dictionary<string, string> args);
}
