using Npgsql;
using Swe.MRP.BL.Abstractions.Repository;
using Swe.MRP.BL.Model;

namespace Swe.MRP.DL.Repository;

public class AdoNetMediaEntryRepository(Database database) : IMediaEntryRepository
{
    private static void LinkGenres(IEnumerable<string> genres, int mediaId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        foreach (string genre in genres)
        {
            using NpgsqlCommand createGenre = new(@"
                INSERT INTO genres (name) VALUES (@n)
                ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
                RETURNING genre_id;
            ", connection, transaction);

            createGenre.Parameters.AddWithValue("n", genre);
            int genreId = (int)createGenre.ExecuteScalar()!;
            
            using NpgsqlCommand link = new(@"
                INSERT INTO media_genres (fk_media_entry_id, fk_genre_id)
                VALUES (@m, @g)
                ON CONFLICT DO NOTHING;
            ", connection, transaction);
            
            link.Parameters.AddWithValue("m", mediaId);
            link.Parameters.AddWithValue("g", genreId);
            link.ExecuteNonQuery();
        }
    }

    public void Create(MediaEntry media)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();
        using NpgsqlTransaction transaction = connection.BeginTransaction();
        
        using NpgsqlCommand create = new(@"
            INSERT INTO media_entries (media_type, title, description, release_year, age_restriction)
            VALUES (@m::media_entry_type, @t, @d, @y, @a)
            RETURNING media_entry_id;
        ", connection, transaction);

        // MediaEntryType needs to be made lowercase to match the ENUM definition in the .sql migration
        // see -> "2026_11_14_create_media_entries.sql"
        create.Parameters.AddWithValue("m", media.MediaType.ToString().ToLowerInvariant());
        create.Parameters.AddWithValue("t", media.Title ?? "");
        create.Parameters.AddWithValue("d", media.Description ?? "");
        create.Parameters.AddWithValue("y", media.ReleaseYear);
        create.Parameters.AddWithValue("a", media.AgeRestriction);
        
        int mediaId = (int)create.ExecuteScalar()!;
        media.Id = mediaId;

        // Deduplicate genres if needed
        IEnumerable<string> genres = (media.Genres ?? [])
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .Distinct()
            .ToList();
        
        LinkGenres(genres, mediaId, connection, transaction);

        transaction.Commit();
        Console.WriteLine($"Media {mediaId} erstellt mit {genres.Count()} Genres");
    }

    public void Delete(int mediaEntryId)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();

        using NpgsqlCommand delete = new(@"
            DELETE FROM media_entries
            WHERE media_entry_id = @id;
        ", connection);

        delete.Parameters.AddWithValue("id", mediaEntryId);
        int affected = delete.ExecuteNonQuery();
        Console.WriteLine($"{affected} rows beeinflusst");
    }

    public MediaEntry? GetById(int mediaId)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();
        using NpgsqlCommand get = new(@"
            SELECT media_entry_id, media_type::text, title, description, release_year, age_restriction
            FROM media_entries
            WHERE media_entry_id = @id;
        ", connection);

        get.Parameters.AddWithValue("id", mediaId);
        using NpgsqlDataReader reader = get.ExecuteReader();

        if (!reader.Read()) return null;

        return new MediaEntry
        {
            Id = reader.GetInt32(0),
            MediaType = Enum.Parse<MediaEntryType>(reader.GetString(1), ignoreCase: true),
            Title = reader.GetString(2),
            Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
            ReleaseYear = reader.GetInt32(4),
            AgeRestriction = reader.GetInt32(5)
        };
    }

    public void Update(int toUpdateId, MediaEntry mediaEdit)
    {
        using NpgsqlConnection connection = database.DataSource.OpenConnection();
        using NpgsqlCommand update = new(@"
            UPDATE media_entries
            SET media_type = @m,
                title = @t,
                description = @d,
                release_year = @y,
                age_restriction = @a
            WHERE media_entry_id = @id;
        ", connection);

        update.Parameters.AddWithValue("id", toUpdateId);
        
        // Same as above (~ comment line 19)
        update.Parameters.AddWithValue("m", mediaEdit.MediaType);
        
        update.Parameters.AddWithValue("t", mediaEdit.Title ?? "");
        update.Parameters.AddWithValue("d", mediaEdit.Description ?? "");
        update.Parameters.AddWithValue("y", mediaEdit.ReleaseYear);
        update.Parameters.AddWithValue("a", mediaEdit.AgeRestriction);
        int affected = update.ExecuteNonQuery();
        Console.WriteLine($"{affected} rows beeinflusst");
    }
}
