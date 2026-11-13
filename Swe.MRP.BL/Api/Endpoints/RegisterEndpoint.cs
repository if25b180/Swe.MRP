using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Abstractions.Api;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Api.Endpoints;

public class RegisterEndpoint : IApiEndpoint
{
    int IApiEndpoint.Version => 1;
    string IApiEndpoint.EndpointName => "/users/register";

    public void Behaviour(IServiceProvider services, Dictionary<string, string> args)
    {
        Console.WriteLine("RegisterEndpoint.Behaviour() wurde aufgerufen.");

        args.TryGetValue("username", out string? username);
        args.TryGetValue("password", out string? password);
        
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

        IUserRepository userRepo = services.GetRequiredService<IUserRepository>();
        userRepo.Add(user);
    }
}
