using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IToolPhotoRepository
{
    Task<int> CountForToolAsync(int toolId, CancellationToken ct);
    Task AddAsync(ToolPhoto photo, CancellationToken ct);
    Task<ToolPhoto?> FindAsync(int toolId, int photoId, CancellationToken ct);
    void Remove(ToolPhoto photo);
    Task SaveChangesAsync(CancellationToken ct);
}