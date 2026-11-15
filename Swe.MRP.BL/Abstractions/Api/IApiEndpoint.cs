using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Swe.MRP.BL.Abstractions.Api;

public interface IApiEndpoint
{
    int Version { get; }
    string EndpointName { get; }
    string EndpointMethod { get; }
    string Behaviour(JsonElement urlParams, JsonElement bodyParams);
}
