using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class MediaEntryUpdate(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;

    public string EndpointName => "/media/{mediaId}";

    public string EndpointMethod => "PUT";

    public string Behaviour(JsonElement urlParams, JsonElement bodyParams)
    {
        if (!urlParams.TryGetProperty("mediaId", out JsonElement idElement)
            || !int.TryParse(idElement.GetString(), out int mediaId))
        {
            return "Ungültige mediaId";
        }

        MediaEntry? entry = services.GetRequiredService<IMediaEntryRepository>().GetById(mediaId);
        if (entry == null) return $"Media Entry Id {mediaId} existiert nicht!";

        // Maybe not all parameters have been set in the request
        if (bodyParams.TryGetProperty("title", out JsonElement element))
            entry.Title = element.GetString();

        if (bodyParams.TryGetProperty("description", out element))
            entry.Description = element.GetString();

        if (bodyParams.TryGetProperty("mediaType", out element))
        {
            if (!Enum.TryParse<MediaEntryType>(element.GetString(), ignoreCase: true, out MediaEntryType parsedEnum))
                return "Konnte mindestens einen Media Type nicht ermitteln. Breche ab.";

            entry.MediaType = parsedEnum;
        }

        if (bodyParams.TryGetProperty("releaseYear", out element))
            entry.ReleaseYear = element.GetInt32();

        if (bodyParams.TryGetProperty("genres", out element))
        {
            string[] genres = element
                .EnumerateArray()
                .Select(e => e.ToString())
                .ToArray()
            ;

            entry.Genres = genres;
        }

        if (bodyParams.TryGetProperty("ageRestriction", out element))
            entry.AgeRestriction = element.GetInt32();

        services.GetRequiredService<IMediaEntryRepository>().Update(mediaId, entry);
        return $"Update eingeleitet. Versuche doch als nächstes ein /api/media/{mediaId} !";
    }
}
