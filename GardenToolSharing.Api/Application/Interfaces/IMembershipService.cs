using GardenToolSharing.Api.Dtos.Memberships;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IMembershipService
{
    Task<MembershipDto> JoinAsync(int userId, JoinToolRequest request, CancellationToken ct);
    Task LeaveAsync(int userId, int membershipId, CancellationToken ct);
    Task<MembershipDto> ApproveAsync(int ownerId, int membershipId, CancellationToken ct);
}
