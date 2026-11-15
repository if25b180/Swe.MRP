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

    public string? Call(string endpointPath, JsonElement urlParams, JsonElement bodyParams)
    {
        Console.WriteLine(endpointPath);

        if (!endpointPath.Contains("api/"))
            return ""; // No particular api endpoint was called

        if (!Endpoints.TryGetValue(endpointPath, out IApiEndpoint? endpoint))
            return $"Endpoint {endpointPath} existiert nicht";

        return endpoint?.Behaviour(urlParams, bodyParams);
    }

    // this function deserves a summary xD
    ///<summary> Matches an endpoint like /api/media/1 to /api/media/{mediaId} </summary>
    public string? Match(string requestRaw, out Dictionary<string, string> variables)
    {
        string? bestKey = null;
        Dictionary<string, string> bestVariables = [];
    
        foreach (KeyValuePair<string, IApiEndpoint> endpoint in Endpoints)
        {
            Dictionary<string, string> foundVariables = [];
            
            string[] requestRawSplit = requestRaw.Trim('/').Split('/');
            string[] endpointSplit = endpoint.Key.Trim('/').Split('/');

            if (endpointSplit.Length != requestRawSplit.Length) continue;

            for (int i = 0; i < endpointSplit.Length; i++)
            {
                string part = endpointSplit[i];
                if (!(part.StartsWith('{') && part.EndsWith('}'))) continue;

                // "{mediaId}" -> "mediaId"
                string name = part[1..^1];
                foundVariables[name] = requestRawSplit[i];

                // Swap the value for the placeholder so the strings can match
                requestRawSplit[i] = part;
            }

            if (endpoint.Key.Trim('/') != string.Join("/", requestRawSplit)) continue;

            // Prefer the route with the fewest variables (/media/search over /media/{mediaId})
            // I don't think its required anywhere in the API spec but better safe than sorry
            if (bestKey == null || foundVariables.Count < bestVariables.Count)
            {
                bestKey = endpoint.Key;
                bestVariables = foundVariables;
            }
        }

        variables = bestVariables;
        return bestKey;
    }
}
