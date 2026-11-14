namespace Swe.MRP.BL.Model;

public class Rating
{
    public int Id { get; set; }
    public int FkUserId { get; set; }
    public int FkMediaEntryId { get; set; }

    public int Stars { get; set; }
    public string? Comment { get; set; }
    public bool Approved { get; set; }
}
