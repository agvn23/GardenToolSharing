namespace GardenToolSharing.Api.Models;

public class Loan
{
    public static readonly TimeSpan EditWindow = TimeSpan.FromHours(24);

    public int Id { get; set; }

    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;

    public int BorrowerId { get; set; }
    public User Borrower { get; set; } = null!;

    public DateOnly BorrowedFrom { get; set; }
    public DateOnly BorrowedUntil { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsActive => ReturnedAt is null;

    /// <summary>Borrowers may edit or delete a loan within 24h of creating it.</summary>
    public bool IsWithinEditWindow(DateTime utcNow) => utcNow - CreatedAt <= EditWindow;
}
