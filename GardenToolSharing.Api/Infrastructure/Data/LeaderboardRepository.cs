using GardenToolSharing.Api.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class LeaderboardRepository(AppDbContext db) : ILeaderboardRepository
{
    public async Task<IReadOnlyList<LenderTotal>> GetTopLendersAsync(int limit, CancellationToken ct)
    {
        // Computed read-only from Loan; no writes, no separate table (FR011)
        var rows = await db.Loans
            .GroupBy(l => new { l.Tool.OwnerId, OwnerName = l.Tool.Owner.DisplayName })
            .Select(g => new { g.Key.OwnerId, g.Key.OwnerName, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.OwnerName)
            .Take(limit)
            .ToListAsync(ct);

        return rows.Select(r => new LenderTotal(r.OwnerId, r.OwnerName, r.Count)).ToList();
    }
}
