using Npgsql;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.DL.Repository;

public class AdoNetUserRepository(Database database) : IUserRepository
{
    private readonly Database _database = database;

    public void Add(User user)
    {
        _database.EnsureOpen();

        NpgsqlCommand register = _database.Command(@"
            INSERT INTO users (username, password)
            VALUES (@u, @p);
        ");

        register.Parameters.AddWithValue("u", user.Name);
        register.Parameters.AddWithValue("p", user.Password);
        
        int r = register.ExecuteNonQuery();
        Console.WriteLine($"{r} rows beeinflusst");
    }

    public void Remove(int id)
    {
        throw new NotImplementedException();
    }
}
