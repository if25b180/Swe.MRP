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

    public void Behaviour(Dictionary<string, string> args)
    {
        Console.WriteLine("LoginEndpoint.Behaviour() wurde aufgerufen.");

        args.TryGetValue("username", out string? username);
        args.TryGetValue("password", out string? password);
        
        if (username == null || password == null)
        {
            Console.WriteLine("WARN: LoginEndpoint->Behaviour() called but no username/password passed.");
            return;
        }

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
