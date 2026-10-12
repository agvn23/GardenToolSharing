namespace GardenToolSharing.Api.Models;

public class ToolPhoto
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;

    public string Caption { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty; // GUID-based; the client's file name is never used
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}