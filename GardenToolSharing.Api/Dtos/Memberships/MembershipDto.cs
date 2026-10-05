using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Dtos.Memberships;

public record MembershipDto(
    int Id,
    int ToolId,
    string ToolName,
    int UserId,
    string UserDisplayName,
    MembershipStatus Status,
    DateTime CreatedAt);
