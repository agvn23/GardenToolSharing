using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

/// <summary>Development-only sample data. Safe to run on every startup: it skips if users exist.</summary>
public class DbSeeder(
    AppDbContext db,
    IPasswordHasher hasher,
    TimeProvider time,
    ILogger<DbSeeder> logger)
{
    // Dev-only credentials, never used outside the Development environment
    private const string DevPassword = "Password123!";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        var now = time.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        var anna = new User
        {
            DisplayName = "Anna",
            Email = "anna@example.com",
            PasswordHash = hasher.Hash(DevPassword),
            CreatedAt = now
        };

        var ben = new User
        {
            DisplayName = "Ben",
            Email = "ben@example.com",
            PasswordHash = hasher.Hash(DevPassword),
            CreatedAt = now
        };

        var wheelbarrow = new Tool
        {
            Owner = anna,
            Name = "Wheelbarrow",
            Description = "Sturdy 100L wheelbarrow, great for soil and compost.",
            Visibility = ToolVisibility.Public,
            Status = ToolStatus.Available,
            // Relative to today so the window is always valid when you seed
            AvailableFrom = today,
            AvailableUntil = today.AddMonths(3),
            CreatedAt = now
        };

        var membership = new Membership
        {
            User = ben,
            Tool = wheelbarrow,
            Status = MembershipStatus.Active,
            CreatedAt = now
        };

        db.AddRange(anna, ben, wheelbarrow, membership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded development data: 2 users, 1 tool, 1 membership (password for both users: {Password})",
            DevPassword);
    }
}
