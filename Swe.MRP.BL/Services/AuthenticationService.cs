using System;
using Swe.MRP.DL;

namespace Swe.MRP.BL.Services;

public class AuthenticationService(Authentication _authentication)
{
    public void Register(string username, string password)
    {
        _authentication.RegisterUser(username, password);
    }

    public void Login(string username, string password)
    {
        _authentication.LoginUser(username, password);
    }
}
