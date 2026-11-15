using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;

namespace Swe.MRP.BL.Endpoints;

public class MediaEntryDelete(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;

    public string EndpointName => "/media/{mediaId}";

    public string EndpointMethod => "DELETE";

    public string Behaviour(JsonElement urlParams, JsonElement bodyParams)
    {
        if (!urlParams.TryGetProperty("mediaId", out JsonElement idElement)
            || !int.TryParse(idElement.GetString(), out int mediaId))
        {
            return "Ungültige mediaId";
        }

        services.GetRequiredService<IMediaEntryRepository>().Delete(mediaId);
        return $"Löschen von {mediaId} eingeleitet. Versuche doch als nächstes ein /api/media/{mediaId} !";
    }
}
