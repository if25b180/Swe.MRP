using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class RegisterEndpoint(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;
    public string EndpointName => "/users/register";

    public string EndpointMethod => "POST";

    public string Behaviour(JsonElement urlParams, JsonElement post)
    {
        string? username = post.GetProperty("username").GetString()!;
        string? password = post.GetProperty("password").GetString()!;
        
        if (username == null || password == null)
            return "WARN: RegisterEndpoint->Behaviour() called but no username/password passed.";

        User user = new()
        {
            Name = username,
            Password = password
        };

        IUserRepository userRepo = services.GetRequiredService<IUserRepository>();
        userRepo.Register(user);

        return "RegisterEndpoint.Behaviour() wurde aufgerufen.";
    }
}
