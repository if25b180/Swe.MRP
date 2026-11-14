using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;


namespace Swe.MRP.DL.Repository;

public class AdoNetUserRepository(Database database) : IUserRepository
{
    private static string AuthToken(User user)
    {
        // Should never be exposed (including being checked in to a git project)
        // I guess for this student project its fine to have this value exposed...
        const string secret = "okVziFRxVaeZVNrPuVuj4KwKgVAa2ho1uiMARtlcYvnuWdeZ";

        SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(secret));
        const int validityHours = 1;

        JsonWebTokenHandler handler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity([
                new(ClaimTypes.NameIdentifier, user.Name)
            ]),
            Expires = DateTime.Now.AddHours(validityHours),
            SigningCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        string token = handler.CreateToken(descriptor);
        return token;
    }

    public void Register(User user)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();
        
        using NpgsqlCommand register = new(@"
            INSERT INTO users (username, password)
            VALUES (@u, @p);
        ", connection);

        register.Parameters.AddWithValue("u", user.Name);
        register.Parameters.AddWithValue("p", user.Password);
        
        int affected = register.ExecuteNonQuery();
        Console.WriteLine($"{affected} rows beeinflusst");
    }

    // Returns login token or null (if login unsuccessful)
    public string? Login(User user)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();
        
        using NpgsqlCommand login = new(@"
            SELECT password FROM users WHERE username = @u
        ", connection);

        login.Parameters.AddWithValue("u", user.Name);
        
        using NpgsqlDataReader reader = login.ExecuteReader();
        if (!reader.Read()) return null; // No user with this username exists

        string? password = reader["password"]?.ToString();
        if (password != user.Password) return null; // Wrong password

        return AuthToken(user);
    }

    public void Remove(int id)
    {
        throw new NotImplementedException();
    }
}
