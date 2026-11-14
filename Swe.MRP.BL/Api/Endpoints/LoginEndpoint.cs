using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Api.Endpoints;

public class LoginEndpoint(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;

    public string EndpointName => "/users/login";

    public IServiceProvider Services => services;

    public void Behaviour(JsonElement get, JsonElement post)
    {
        Console.WriteLine("LoginEndpoint.Behaviour() wurde aufgerufen.");

        // Null surpressing here may be a bit smelly...
        string username = post.GetProperty("username").GetString()!;
        string password = post.GetProperty("password").GetString()!;

        User user = new()
        {
            Name = username,
            Password = password
        };

        IUserRepository userRepo = Services.GetRequiredService<IUserRepository>();
        string? token = userRepo.Login(user);
        Console.WriteLine($"Dein Token: {token}");
    }
}
