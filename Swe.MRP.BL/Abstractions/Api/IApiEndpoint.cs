using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Abstractions.Api;

public interface IApiEndpoint
{
    IServiceProvider Services { get; }
    int Version { get; }
    string EndpointName { get; }
    string EndpointMethod { get; }
    void Behaviour(JsonElement urlParams, JsonElement bodyParams);
}
