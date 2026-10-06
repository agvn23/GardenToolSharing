using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Tools;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class ToolService(IToolRepository tools, TimeProvider time) : IToolService
{
    public async Task<ToolDto> CreateAsync(int ownerId, CreateToolRequest request, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var from = request.AvailableFrom!.Value;
        var until = request.AvailableUntil!.Value;

        if (until < DateOnly.FromDateTime(now))
            throw DomainValidationException.ForField("availableUntil", "Available until can't be in the past.");

        var tool = new Tool
        {
            OwnerId = ownerId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Visibility = request.Visibility,
            Status = ToolStatus.Available,
            AvailableFrom = from,
            AvailableUntil = until,
            CreatedAt = now
        };

        await tools.AddAsync(tool, ct);

        // Reload so the Owner navigation is populated for the response
        var created = await tools.FindVisibleToAsync(tool.Id, ownerId, ct)
                      ?? throw new InvalidOperationException("Created tool could not be reloaded.");
        return ToDto(created, ownerId);
    }

    public async Task<IReadOnlyList<ToolDto>> ListAsync(
        int userId, ToolVisibility? visibility, ToolStatus? status, bool mineOnly, CancellationToken ct)
    {
        var list = await tools.ListVisibleToAsync(userId, visibility, status, mineOnly, ct);
        return list.Select(t => ToDto(t, userId)).ToList();
    }

    public async Task<ToolDto> GetAsync(int userId, int toolId, CancellationToken ct)
    {
        // Private tools the user has no relation to look exactly like tools that don't exist
        var tool = await tools.FindVisibleToAsync(toolId, userId, ct)
                   ?? throw new NotFoundException("Tool not found.");
        return ToDto(tool, userId);
    }

private static ToolDto ToDto(Tool t, int viewerId)
{
    var activeLoan = t.Loans.FirstOrDefault(l => l.ReturnedAt == null);
    var canSeeBorrower = activeLoan is not null
        && (t.OwnerId == viewerId || activeLoan.BorrowerId == viewerId);

    return new ToolDto(
        t.Id, t.Name, t.Description, t.Visibility, t.Status,
        t.AvailableFrom, t.AvailableUntil, t.OwnerId, t.Owner.DisplayName, t.CreatedAt,
        canSeeBorrower ? activeLoan!.Borrower.DisplayName : null);
}
}
