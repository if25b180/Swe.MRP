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
    public string[]? Genre { get; set; }
    public int AgeRestriction { get; set; }
}
