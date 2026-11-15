namespace Swe.MRP.BL.Model;

public enum MediaEntryType
{
    MOVIE, SERIES, GAME
}

public class MediaEntry
{
    public int Id { get; set; }
    public MediaEntryType MediaType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int ReleaseYear { get; set; }
    public string[]? Genres { get; set; }
    public int AgeRestriction { get; set; }

    public string GetString()
    {
        // Please excuse this ugly formatting; it avoids distracting whitespace 
        return
$@"MediaEntry {{
    Id: `{Id}`,
    MediaType: `{MediaType}`,
    Title: `{Title}`,
    Description: `{Description}`,
    ReleaseYear: `{ReleaseYear}`,
    Genres: `{string.Join(", ", Genres ?? [])}`,
    AgeRestriction: `{AgeRestriction}`
}}
";
    }
}
