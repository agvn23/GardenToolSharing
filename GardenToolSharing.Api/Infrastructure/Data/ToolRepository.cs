using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class ToolRepository(AppDbContext db) : IToolRepository
{
    public async Task AddAsync(Tool tool, CancellationToken ct)
    {
        db.Tools.Add(tool);
        await db.SaveChangesAsync(ct);
    }

    public Task<Tool?> FindByIdAsync(int toolId, CancellationToken ct) =>
        db.Tools.AsNoTracking().FirstOrDefaultAsync(t => t.Id == toolId && !t.IsDeleted, ct);

    public Task<Tool?> FindTrackedByIdAsync(int toolId, CancellationToken ct) =>
        db.Tools.FirstOrDefaultAsync(t => t.Id == toolId && !t.IsDeleted, ct);

    public Task<Tool?> FindTrackedIncludingDeletedAsync(int toolId, CancellationToken ct) =>
    db.Tools.FirstOrDefaultAsync(t => t.Id == toolId, ct);

    public async Task<IReadOnlyList<Tool>> ListHiddenOwnedByAsync(int ownerId, CancellationToken ct) =>
        await db.Tools
            .AsNoTracking()
            .Include(t => t.Owner)
            .Where(t => t.OwnerId == ownerId && t.IsDeleted)
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private IQueryable<Tool> VisibleTo(int userId) =>
        db.Tools.Where(t => !t.IsDeleted && (
            t.Visibility == ToolVisibility.Public ||
            t.OwnerId == userId ||
            t.Memberships.Any(m => m.UserId == userId)));
    public Task<Tool?> FindVisibleToAsync(int toolId, int userId, CancellationToken ct) =>
        VisibleTo(userId)
            .AsNoTracking()
            .Include(t => t.Owner)
            .Include(t => t.Loans.Where(l => l.ReturnedAt == null)).ThenInclude(l => l.Borrower)
            .Include(t => t.Memberships.Where(m => m.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == toolId, ct);

    public async Task<IReadOnlyList<Tool>> ListVisibleToAsync(
        int userId, ToolVisibility? visibility, ToolStatus? status, bool mineOnly, CancellationToken ct)
    {
        var query = VisibleTo(userId);

        if (visibility is { } v)
            query = query.Where(t => t.Visibility == v);

        if (status is { } s)
            query = query.Where(t => t.Status == s);

        if (mineOnly)
        {
            query = query.Where(t =>
                t.OwnerId == userId ||
                t.Loans.Any(l => l.BorrowerId == userId && l.ReturnedAt == null));
        }

        return await query
            .AsNoTracking()
            .Include(t => t.Owner)
            .Include(t => t.Loans.Where(l => l.ReturnedAt == null)).ThenInclude(l => l.Borrower)
            .Include(t => t.Memberships.Where(m => m.UserId == userId))
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .ToListAsync(ct);
    }

}
