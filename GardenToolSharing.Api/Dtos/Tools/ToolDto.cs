using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Dtos.Tools;

public record ToolDto(
    int Id,
    string Name,
    string? Description,
    ToolVisibility Visibility,
    ToolStatus Status,
    DateOnly AvailableFrom,
    DateOnly AvailableUntil,
    int OwnerId,
    string OwnerName,
    DateTime CreatedAt);
