using GardenToolSharing.Api.Dtos.Tools;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IToolService
{
    Task<ToolDto> CreateAsync(int ownerId, CreateToolRequest request, CancellationToken ct);

    Task<IReadOnlyList<ToolDto>> ListAsync(
        int userId, ToolVisibility? visibility, ToolStatus? status, bool mineOnly, CancellationToken ct);

    Task<ToolDto> GetAsync(int userId, int toolId, CancellationToken ct);
}
