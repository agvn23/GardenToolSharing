using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Memberships;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class MembershipService(
    IMembershipRepository memberships,
    IToolRepository tools,
    TimeProvider time) : IMembershipService
{
    public async Task<MembershipDto> JoinAsync(int userId, JoinToolRequest request, CancellationToken ct)
    {
        var toolId = request.ToolId!.Value;

        // Any tool by id, regardless of visibility: a user needs to be able to request
        // to join a private tool (e.g. shared by link) before they have a membership on it.
        var tool = await tools.FindByIdAsync(toolId, ct)
                   ?? throw new NotFoundException("Tool not found.");

        if (tool.OwnerId == userId)
            throw new ConflictException("You already own this tool.");

        if (await memberships.FindByUserAndToolAsync(userId, toolId, ct) is not null)
            throw new ConflictException("You already have a membership on this tool.");

        var membership = new Membership
        {
            UserId = userId,
            ToolId = toolId,
            Status = tool.Visibility == ToolVisibility.Public
                ? MembershipStatus.Active
                : MembershipStatus.Pending,
            CreatedAt = time.GetUtcNow().UtcDateTime
        };

        await memberships.AddAsync(membership, ct);

        // Reload so Tool/User navigations are populated for the response
        var created = await memberships.FindByIdAsync(membership.Id, ct)
                      ?? throw new InvalidOperationException("Created membership could not be reloaded.");
        return ToDto(created);
    }

    public async Task LeaveAsync(int userId, int membershipId, CancellationToken ct)
    {
        var membership = await memberships.FindByIdAsync(membershipId, ct);

        // Hide existence of memberships that aren't the caller's own
        if (membership is null || membership.UserId != userId)
            throw new NotFoundException("Membership not found.");

        memberships.Remove(membership);
        await memberships.SaveChangesAsync(ct);
    }

    public async Task<MembershipDto> ApproveAsync(int ownerId, int membershipId, CancellationToken ct)
    {
        var membership = await memberships.FindByIdAsync(membershipId, ct);

        // Hide existence of memberships on tools the caller doesn't own
        if (membership is null || membership.Tool.OwnerId != ownerId)
            throw new NotFoundException("Membership not found.");

        if (membership.Status != MembershipStatus.Pending)
            throw new ConflictException("Only pending membership requests can be approved.");

        membership.Status = MembershipStatus.Active;
        await memberships.SaveChangesAsync(ct);

        return ToDto(membership);
    }

    private static MembershipDto ToDto(Membership m) => new(
        m.Id, m.ToolId, m.Tool.Name, m.UserId, m.User.DisplayName, m.Status, m.CreatedAt);
}
