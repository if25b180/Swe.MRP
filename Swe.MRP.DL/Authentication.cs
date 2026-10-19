using Npgsql;

namespace Swe.MRP.DL;

public class Authentication(Database database)
{
    private readonly Database _database = database;

    public void RegisterUser(string username, string password)
    {
        NpgsqlCommand register = _database.Command(@"
            INSERT INTO users (username, password)
            VALUES ('@u', '@p');
        ");

        register.Parameters.AddWithValue("u", username);
        register.Parameters.AddWithValue("p", password);

        register.ExecuteNonQuery();
    }

    public void LoginUser(string username, string password)
    {
        
    }
}
