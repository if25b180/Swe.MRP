using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Endpoints;

public class LoginEndpoint(IServiceProvider services) : IApiEndpoint
{
    public int Version => 1;

    public string EndpointName => "/users/login";

    public IServiceProvider Services => services;

    public string EndpointMethod => "POST";

    public void Behaviour(JsonElement get, JsonElement post)
    {
        Console.WriteLine("LoginEndpoint.Behaviour() wurde aufgerufen.");

        string username = post.GetProperty("username").ToString();
        string password = post.GetProperty("password").ToString();

        User user = new()
        {
            Name = username,
            Password = password
        };

        IUserRepository userRepo = Services.GetRequiredService<IUserRepository>();
        string? token = userRepo.Login(user);
        if (token != null)
            Console.WriteLine($"Dein Token: {token}");
        else
            Console.WriteLine("Falscher/s Benutzernahme oder Passwort");
    }
}
