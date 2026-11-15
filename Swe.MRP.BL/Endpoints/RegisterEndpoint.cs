using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class RegisterEndpoint(IServiceProvider services) : IApiEndpoint
{
    public IServiceProvider Services => services;

    public int Version => 1;
    public string EndpointName => "/users/register";

    public string EndpointMethod => "POST";

    public void Behaviour(JsonElement urlParams, JsonElement post)
    {
        Console.WriteLine("RegisterEndpoint.Behaviour() wurde aufgerufen.");

        string? username = post.GetProperty("username").GetString()!;
        string? password = post.GetProperty("password").GetString()!;
        
        if (username == null || password == null)
        {
            Console.WriteLine("WARN: RegisterEndpoint->Behaviour() called but no username/password passed.");
            return;
        }

        User user = new()
        {
            Name = username,
            Password = password
        };

        IUserRepository userRepo = Services.GetRequiredService<IUserRepository>();
        userRepo.Register(user);
    }
}
