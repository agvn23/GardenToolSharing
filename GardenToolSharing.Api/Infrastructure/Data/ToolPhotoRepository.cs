using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class ToolPhotoRepository(AppDbContext db) : IToolPhotoRepository
{
    public Task<int> CountForToolAsync(int toolId, CancellationToken ct) =>
        db.ToolPhotos.CountAsync(p => p.ToolId == toolId, ct);

    public async Task AddAsync(ToolPhoto photo, CancellationToken ct)
    {
        db.ToolPhotos.Add(photo);
        await db.SaveChangesAsync(ct);
    }

    public Task<ToolPhoto?> FindAsync(int toolId, int photoId, CancellationToken ct) =>
        db.ToolPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.ToolId == toolId, ct);

    public void Remove(ToolPhoto photo) => db.ToolPhotos.Remove(photo);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}