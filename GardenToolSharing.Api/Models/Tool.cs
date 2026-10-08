namespace GardenToolSharing.Api.Models;

public class Tool
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ToolVisibility Visibility { get; set; } = ToolVisibility.Public;
    public ToolStatus Status { get; set; } = ToolStatus.Available;

    public DateOnly AvailableFrom { get; set; }
    public DateOnly AvailableUntil { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Membership> Memberships { get; set; } = [];
    public List<Loan> Loans { get; set; } = [];

    /// <summary>True when the requested period lies inside the owner's availability window.</summary>
    public bool IsWithinWindow(DateOnly from, DateOnly until) =>
        from >= AvailableFrom && until <= AvailableUntil;

    /// <summary>Soft delete: hidden from everyone but its owner, who can restore it.</summary>
    public bool IsDeleted { get; set; }
}

