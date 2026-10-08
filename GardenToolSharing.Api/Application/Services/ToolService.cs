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

    public async Task<ToolDto> UpdateAsync(int ownerId, int toolId, CreateToolRequest request, CancellationToken ct)
    {
    var tool = await RequireOwnedIdleToolAsync(ownerId, toolId, ct);

    var now = time.GetUtcNow().UtcDateTime;
    var until = request.AvailableUntil!.Value;

    // Only enforce "not in the past" when the end date is actually being changed
    if (until != tool.AvailableUntil && until < DateOnly.FromDateTime(now))
        throw DomainValidationException.ForField("availableUntil", "Available until can't be in the past.");

    tool.Name = request.Name.Trim();
    tool.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
    tool.Visibility = request.Visibility;
    tool.AvailableFrom = request.AvailableFrom!.Value;
    tool.AvailableUntil = until;

    await tools.SaveChangesAsync(ct);

    var updated = await tools.FindVisibleToAsync(toolId, ownerId, ct)
        ?? throw new InvalidOperationException("Updated tool could not be reloaded.");
    return ToDto(updated, ownerId);
    }

    public async Task DeleteAsync(int ownerId, int toolId, CancellationToken ct)
    {
        var tool = await RequireOwnedIdleToolAsync(ownerId, toolId, ct);
        tool.IsDeleted = true;
        await tools.SaveChangesAsync(ct);
    }

    public async Task<ToolDto> RestoreAsync(int ownerId, int toolId, CancellationToken ct)
    {
        var tool = await tools.FindTrackedIncludingDeletedAsync(toolId, ct)
            ?? throw new NotFoundException("Tool not found.");

    if (tool.OwnerId != ownerId)
        throw new ForbiddenException("Only the tool's owner can restore it.");

    if (!tool.IsDeleted)
        throw new ConflictException("This tool hasn't been removed.");

    tool.IsDeleted = false;
    await tools.SaveChangesAsync(ct);

    var restored = await tools.FindVisibleToAsync(toolId, ownerId, ct)
        ?? throw new InvalidOperationException("Restored tool could not be reloaded.");
    return ToDto(restored, ownerId);
    }

    public async Task<IReadOnlyList<ToolDto>> ListHiddenAsync(int ownerId, CancellationToken ct)
    {
        var list = await tools.ListHiddenOwnedByAsync(ownerId, ct);
        return list.Select(t => ToDto(t, ownerId)).ToList();
    }

    private async Task<Tool> RequireOwnedIdleToolAsync(int ownerId, int toolId, CancellationToken ct)
    {
        var tool = await tools.FindTrackedByIdAsync(toolId, ct)
            ?? throw new NotFoundException("Tool not found.");

    if (tool.OwnerId != ownerId)
        throw new ForbiddenException("Only the tool's owner can change it.");

    if (tool.Status != ToolStatus.Available)
        throw new ConflictException("A tool that's lent out can't be changed. Mark it as returned first.");

    return tool;
    }

    private static ToolDto ToDto(Tool t, int viewerId)
    {
        var activeLoan = t.Loans.FirstOrDefault(l => l.ReturnedAt == null);
        var canSeeBorrower = activeLoan is not null
            && (t.OwnerId == viewerId || activeLoan.BorrowerId == viewerId);


        var myMembership = t.Memberships.FirstOrDefault(m => m.UserId == viewerId);

        var myLoan = activeLoan is not null && activeLoan.BorrowerId == viewerId
            ? new MyLoanDto(
                activeLoan.Id, activeLoan.BorrowedFrom, activeLoan.BorrowedUntil, activeLoan.Note,
                DateTime.SpecifyKind(activeLoan.CreatedAt, DateTimeKind.Utc).Add(Loan.EditWindow))
        : null;

    return new ToolDto(
        t.Id, t.Name, t.Description, t.Visibility, t.Status,
        t.AvailableFrom, t.AvailableUntil, t.OwnerId, t.Owner.DisplayName, t.CreatedAt,
        canSeeBorrower ? activeLoan!.Borrower.DisplayName : null,
        myMembership?.Status,
        myLoan);   
    }
}
