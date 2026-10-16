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

    private List<string> GetAppliedSchemas()
    {
        EnsureOpen();

        NpgsqlCommand schemasRan = new("SELECT to_regclass('public.applied_schemas') IS NOT NULL", _connection);
        if (!(bool) schemasRan.ExecuteScalar()!)
        {
            return [];
        }

        List<string> appliedSchemas = [];
        NpgsqlCommand schemasFilenames = new("SELECT filename from applied_schemas", _connection);
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

        string?[] schemaFiles = [.. Directory.GetFiles(_schemasFolder, "*.sql", SearchOption.AllDirectories)];
        return schemaFiles;
    }

    public void ApplyMissingSchemas()
    {
        EnsureOpen();

        NpgsqlCommand createAppliedSchemasLog = new(
            @"
            CREATE TABLE IF NOT EXISTS applied_schemas(
                filename text PRIMARY KEY
            )
            "
        , _connection);
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

                NpgsqlCommand schemaRun = new(File.ReadAllText(path), _connection, transaction);
                schemaRun.ExecuteNonQuery();

                NpgsqlCommand logSchema = new("INSERT INTO applied_schemas (filename) VALUES (@f)", _connection, transaction);
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