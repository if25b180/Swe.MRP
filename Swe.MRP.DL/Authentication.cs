using Npgsql;

namespace Swe.MRP.DL;

public class Authentication(Database database)
{
    private readonly Database _database = database;

    public void RegisterUser(string username, string password)
    {
        _database.EnsureOpen();

        NpgsqlCommand register = _database.Command(@"
            INSERT INTO users (username, password)
            VALUES (@u, @p);
        ");

        register.Parameters.AddWithValue("u", username);
        register.Parameters.AddWithValue("p", password);
        
        int r = register.ExecuteNonQuery();
        Console.WriteLine($"{r} rows beeinflusst");
    }

    public void LoginUser(string username, string password)
    {
        
    }
}
