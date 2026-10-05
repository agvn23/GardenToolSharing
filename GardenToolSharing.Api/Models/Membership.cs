namespace GardenToolSharing.Api.Models;

public class Membership
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;

    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public DateTime CreatedAt { get; set; }
}
