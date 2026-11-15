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

    public string EndpointMethod => "POST";

    public string Behaviour(JsonElement get, JsonElement post)
    {
        string response = "LoginEndpoint.Behaviour() wurde aufgerufen.\n";

        string username = post.GetProperty("username").ToString();
        string password = post.GetProperty("password").ToString();

        User user = new()
        {
            Name = username,
            Password = password
        };

        IUserRepository userRepo = services.GetRequiredService<IUserRepository>();
        string? token = userRepo.Login(user);
        if (token != null)
            response += $"Dein Token: {token}";
        else
            response += "Falscher/s Benutzernahme oder Passwort";

        return response;
    }
}
