using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Services;

namespace Swe.MRP.BL.Api.Endpoints;

public class RegisterEndpoint : IApiEndpoint
{
    int IApiEndpoint.Version => 1;
    string IApiEndpoint.EndpointName => "/users/register";

    public void Behaviour(ServiceProvider services, Dictionary<string, string> args)
    {
        Console.WriteLine("RegisterEndpoint.Behaviour() wurde aufgerufen.");

        /* TODO: implement args parsing
        * args parsing doesn't work right now, so we'll comment it out for now,
        * avoids an exception as of now

        args.TryGetValue("username", out string? username);
        args.TryGetValue("password", out string? password);

        AuthenticationService authentication = services.GetRequiredService<AuthenticationService>();
        
        if (username == null || password == null)
        {
            Console.WriteLine("WARN: RegisterEndpoint->Behaviour() called but no username/password passed.");
            return;
        }

        authentication.Register(username, password);
        */
    }
}
