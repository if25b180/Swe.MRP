using Swe.MRP.BL.Abstractions.Api;

namespace Swe.MRP.BL.Api.Endpoints;

public class MediaEntryCreate(IServiceProvider services) : IApiEndpoint
{
    public IServiceProvider Services => services;

    public int Version => 1;

    public string EndpointName => "/media";

    public void Behaviour(Dictionary<string, string> args)
    {
        
    }
}
