using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class MediaEntryCreate(IServiceProvider services) : IApiEndpoint
{
    public IServiceProvider Services => services;

    public int Version => 1;

    public string EndpointName => "/media";


    public string EndpointMethod => "POST";

    public void Behaviour(JsonElement urlParams, JsonElement post)
    {
        string title = post.GetProperty("title").ToString();
        string description = post.GetProperty("description").ToString();
        int releaseYear = post.GetProperty("releaseYear").GetInt32();
        int ageRestriction = post.GetProperty("ageRestriction").GetInt32();

        string[] genres = post.GetProperty("genres")
            .EnumerateArray()
            .Select(e => e.ToString())
            .ToArray()
        ;

        Enum.TryParse<MediaEntryType>(
            post.GetProperty("mediaType").GetString(),
            ignoreCase: true,
            out MediaEntryType type
        );

        MediaEntry mediaEntry = new()
        {
            MediaType = type,
            Title = title,
            Description = description,
            ReleaseYear = releaseYear,
            AgeRestriction = ageRestriction,
            Genres = genres
        };

        IMediaEntryRepository mediaRepo = Services.GetRequiredService<IMediaEntryRepository>();
        mediaRepo.Create(mediaEntry);
    }
}
