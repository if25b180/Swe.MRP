using System.Text.Json;
using Swe.MRP.BL.Abstractions.Api;

namespace Swe.MRP.BL.Api.Endpoints;

public class MediaEntryCreate(IServiceProvider services) : IApiEndpoint
{
    public IServiceProvider Services => services;

    public int Version => 1;

    public string EndpointName => "/media";

    public void Behaviour(JsonElement get, JsonElement post)
    {/*
        args.TryGetValue("title", out string? title);
        args.TryGetValue("title", out string? title);
        args.TryGetValue("title", out string? title);
        args.TryGetValue("title", out string? title);
        args.TryGetValue("title", out string? title);
        args.TryGetValue("title", out string? title);*/
    }
}
