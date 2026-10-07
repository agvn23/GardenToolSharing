using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class MembershipRepository(AppDbContext db) : IMembershipRepository
{
    public async Task AddAsync(Membership membership, CancellationToken ct)
    {
        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);
    }

    public Task<Membership?> FindByIdAsync(int id, CancellationToken ct) =>
        db.Memberships
            .Include(m => m.User)
            .Include(m => m.Tool)
            .ThenInclude(t => t.Owner)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Membership?> FindByUserAndToolAsync(int userId, int toolId, CancellationToken ct) =>
        db.Memberships.FirstOrDefaultAsync(m => m.UserId == userId && m.ToolId == toolId, ct);

    public void Remove(Membership membership) => db.Memberships.Remove(membership);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Membership>> ListPendingForOwnerAsync(int ownerId, CancellationToken ct) =>
    await db.Memberships
        .AsNoTracking()
        .Include(m => m.User)
        .Include(m => m.Tool)
        .Where(m => m.Tool.OwnerId == ownerId && m.Status == MembershipStatus.Pending)
        .OrderBy(m => m.CreatedAt)
        .ThenBy(m => m.Id)
        .ToListAsync(ct);
}
