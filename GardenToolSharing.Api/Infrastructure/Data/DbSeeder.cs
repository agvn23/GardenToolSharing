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

        User NewUser(string name) => new()
        {
            DisplayName = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            PasswordHash = hasher.Hash(DevPassword),
            CreatedAt = now
        };

        // Availability starts 30 days back so the returned loan below sits inside the window
        Tool NewTool(User owner, string name, string description, ToolVisibility visibility) => new()
        {
            Owner = owner,
            Name = name,
            Description = description,
            Visibility = visibility,
            Status = ToolStatus.Available,
            AvailableFrom = today.AddDays(-30),
            AvailableUntil = today.AddMonths(3),
            CreatedAt = now
        };

        Membership NewMembership(User user, Tool tool, MembershipStatus status) => new()
        {
            User = user,
            Tool = tool,
            Status = status,
            CreatedAt = now
        };

        var anna = NewUser("Anna");
        var ben = NewUser("Ben");
        var dave = NewUser("Dave");
        var erin = NewUser("Erin");

        var wheelbarrow = NewTool(anna, "Wheelbarrow",
            "Sturdy 100L wheelbarrow, great for soil and compost.", ToolVisibility.Public);
        var hedgeTrimmer = NewTool(anna, "Hedge trimmer",
            "Cordless hedge trimmer with a spare battery.", ToolVisibility.Private);
        var hammer = NewTool(ben, "Hammer",
            "Claw hammer, good for fences and sheds.", ToolVisibility.Public);
        var carJack = NewTool(dave, "Car jack",
            "Hydraulic trolley jack, 2 tonne.", ToolVisibility.Public);
        var chainSaw = NewTool(dave, "Chain saw",
            "Petrol chain saw. Ask first, please.", ToolVisibility.Private);
        var sewingMachine = NewTool(erin, "Sewing machine",
            "Basic sewing machine with a box of bobbins.", ToolVisibility.Private);

        // Memberships: public tools join automatically, private ones need the owner's approval
        var memberships = new[]
        {
            NewMembership(ben, wheelbarrow, MembershipStatus.Active),
            NewMembership(dave, hedgeTrimmer, MembershipStatus.Active),
            NewMembership(dave, hammer, MembershipStatus.Active),
            NewMembership(ben, chainSaw, MembershipStatus.Pending)   // shows on Dave's Requests page
        };

        // Two loans out now (Anna's tools) and one already returned (Ben's hammer)
        var loans = new[]
        {
            new Loan
            {
                Tool = wheelbarrow, Borrower = ben,
                BorrowedFrom = today, BorrowedUntil = today.AddDays(14),
                Note = "For the new vegetable beds", CreatedAt = now
            },
            new Loan
            {
                Tool = hedgeTrimmer, Borrower = dave,
                BorrowedFrom = today, BorrowedUntil = today.AddDays(14),
                Note = "For the front hedge", CreatedAt = now
            },
            new Loan
            {
                Tool = hammer, Borrower = dave,
                BorrowedFrom = today.AddDays(-10), BorrowedUntil = today.AddDays(-3),
                ReturnedAt = now.AddDays(-3), CreatedAt = now.AddDays(-10)
            }
        };

        wheelbarrow.Status = ToolStatus.Lent;
        hedgeTrimmer.Status = ToolStatus.Lent;

        db.AddRange(anna, ben, dave, erin);
        db.AddRange(wheelbarrow, hedgeTrimmer, hammer, carJack, chainSaw, sewingMachine);
        db.AddRange(memberships);
        db.AddRange(loans);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded development data: 4 users, 6 tools, 4 memberships, 3 loans (password for all users: {Password})",
            DevPassword);
    }
}
