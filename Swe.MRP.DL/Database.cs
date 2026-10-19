namespace Swe.MRP.DL;

using Npgsql;

public class Database(string connectionString)
{
    private readonly NpgsqlConnection _connection = new(connectionString);
    private readonly string _schemasFolder = Path.Combine(AppContext.BaseDirectory, "Schemas");

    private void EnsureOpen()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();
    }

    public NpgsqlCommand Command(string sqlString, NpgsqlTransaction? _transaction = null)
    {
        return new NpgsqlCommand(sqlString, _connection, _transaction);
    }

    private List<string> GetAppliedSchemas()
    {
        EnsureOpen();

        NpgsqlCommand schemasRan = Command("SELECT to_regclass('public.applied_schemas') IS NOT NULL");
        if (!(bool) schemasRan.ExecuteScalar()!)
        {
            return [];
        }

        List<string> appliedSchemas = [];
        NpgsqlCommand schemasFilenames = Command("SELECT filename from applied_schemas");
        NpgsqlDataReader reader = schemasFilenames.ExecuteReader();
        while (reader.Read())
        {
            appliedSchemas.Add(reader.GetString(0));
        }
        reader.Close();
        return appliedSchemas;
    }

    private string?[] GetSchemaFiles()
    {
        EnsureOpen();

        string?[] schemaFiles = Directory.GetFiles(_schemasFolder, "*.sql", SearchOption.AllDirectories);
        return schemaFiles;
    }

    public void ApplyMissingSchemas()
    {
        EnsureOpen();

        NpgsqlCommand createAppliedSchemasLog = Command(
            @"
            CREATE TABLE IF NOT EXISTS applied_schemas(
                filename text PRIMARY KEY
            )
            "
        );
        createAppliedSchemasLog.ExecuteNonQuery();

        List<string> appliedSchemas = GetAppliedSchemas();

        using NpgsqlTransaction transaction = _connection.BeginTransaction();
        foreach (string? path in GetSchemaFiles())
        {
            string? file = Path.GetFileName(path);

            try 
            {
                if (file == null || path == null)
                {
                    continue;
                }

                if (appliedSchemas.Contains(file))
                {
                    continue;
                }

                NpgsqlCommand schemaRun = Command(File.ReadAllText(path), transaction);
                schemaRun.ExecuteNonQuery();

                NpgsqlCommand logSchema = Command("INSERT INTO applied_schemas (filename) VALUES (@f)", transaction);
                logSchema.Parameters.AddWithValue("f", file);
                logSchema.ExecuteNonQuery();

                transaction.Commit();
            } catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}