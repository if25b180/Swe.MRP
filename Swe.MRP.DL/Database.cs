namespace Swe.MRP.DL;

using Npgsql;

public class Database
{
    NpgsqlConnection Connection;

    public Database(string connectionString)
    {
        Connection = new NpgsqlConnection(connectionString);
    }
}