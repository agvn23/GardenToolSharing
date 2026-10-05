namespace GardenToolSharing.Api.Models;

public class User
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<Tool> OwnedTools { get; set; } = [];
    public List<Membership> Memberships { get; set; } = [];
    public List<Loan> Loans { get; set; } = [];
}
