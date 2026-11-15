using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class MediaEntryGetById(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;

    public string EndpointName => "/media/{mediaId}";

    public string EndpointMethod => "GET";

    public string Behaviour(JsonElement urlParams, JsonElement bodyParams)
    {
        if (!urlParams.TryGetProperty("mediaId", out JsonElement idElement)
            || !int.TryParse(idElement.GetString(), out int mediaId))
        {
            return "Ungültige mediaId";
        }

        Console.WriteLine(mediaId);
        MediaEntry? entry = services.GetRequiredService<IMediaEntryRepository>().GetById(mediaId);
        return entry?.GetString() ?? $"Kein Eintrag mit id {mediaId} gefunden";
    }
}
