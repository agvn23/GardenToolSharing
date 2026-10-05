using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership, CancellationToken ct);

    /// <summary>Membership by id, with User and Tool (and Tool.Owner) loaded.</summary>
    Task<Membership?> FindByIdAsync(int id, CancellationToken ct);

    Task<Membership?> FindByUserAndToolAsync(int userId, int toolId, CancellationToken ct);

    void Remove(Membership membership);

    Task SaveChangesAsync(CancellationToken ct);
}
