namespace Swe.MRP.DL;

using Npgsql;
using Swe.MRP.BL.Model;

public class Database
{
    private readonly string _schemasFolder = Path.Combine(AppContext.BaseDirectory, "Schemas");
    public NpgsqlDataSource DataSource { get; }

    public Database(string connectionString)
    {
        NpgsqlDataSourceBuilder builder = new(connectionString);
        builder.MapEnum<MediaEntryType>("media_entry_type");
        DataSource = builder.Build();
    }

    private List<string> GetAppliedSchemas()
    {
        using NpgsqlConnection connection = DataSource.OpenConnection();

        NpgsqlCommand schemasRan = new("SELECT to_regclass('public.applied_schemas') IS NOT NULL", connection);
        if (!(bool) schemasRan.ExecuteScalar()!)
        {
            return [];
        }

        List<string> appliedSchemas = [];
        NpgsqlCommand schemasFilenames = new("SELECT filename from applied_schemas", connection);
        NpgsqlDataReader reader = schemasFilenames.ExecuteReader();
        while (reader.Read())
        {
            appliedSchemas.Add(reader.GetString(0));
        }
        reader.Close();
        return appliedSchemas;
    }

    private string[] GetSchemaFiles()
    {
        return Directory.GetFiles(_schemasFolder, "*.sql", SearchOption.AllDirectories)
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToArray()
        ;
    }

    public void ApplyMissingSchemas()
    {       
        using NpgsqlConnection connection = DataSource.OpenConnection();

        NpgsqlCommand createAppliedSchemasLog = new(
            @"
            CREATE TABLE IF NOT EXISTS applied_schemas(
                filename text PRIMARY KEY
            )
            ", connection
        );
        createAppliedSchemasLog.ExecuteNonQuery();

        List<string> appliedSchemas = GetAppliedSchemas();
        
        foreach (string path in GetSchemaFiles())
        {
            string file = Path.GetFileName(path);
            if (appliedSchemas.Contains(file)) continue;

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try 
            {
                NpgsqlCommand schemaRun = new(File.ReadAllText(path), connection, transaction);
                schemaRun.ExecuteNonQuery();

                NpgsqlCommand logSchema = new("INSERT INTO applied_schemas (filename) VALUES (@f)", connection, transaction);
                logSchema.Parameters.AddWithValue("f", file);
                logSchema.ExecuteNonQuery();

                transaction.Commit();
                connection.ReloadTypes(); // helps if any custom types were defined e. g. 'media_entry_type'
                Console.WriteLine($"Schema {file} wurde ausgeführt.");
            } catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}