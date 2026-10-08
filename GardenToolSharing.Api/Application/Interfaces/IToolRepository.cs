using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IToolRepository
{
    Task AddAsync(Tool tool, CancellationToken ct);

    /// <summary>Raw lookup by id, untracked, no visibility restriction. Used where the caller must
    /// reference a private tool they don't yet have a relation to (e.g. joining by link).</summary>
    Task<Tool?> FindByIdAsync(int toolId, CancellationToken ct);

    /// <summary>Raw lookup by id, tracked, no visibility restriction. Used when the tool's Status
    /// (or another field) is about to be mutated and saved as part of the current operation.</summary>
    Task<Tool?> FindTrackedByIdAsync(int toolId, CancellationToken ct);

    /// <summary>
    /// Returns the tool (with Owner) only if the user may see it: public, owned by the user,
    /// or the user has a membership on it (any status). Otherwise null.
    /// </summary>
    Task<Tool?> FindVisibleToAsync(int toolId, int userId, CancellationToken ct);

    /// <summary>
    /// Tools the user may see, optionally filtered. mineOnly = owned by the user or joined (active membership).
    /// </summary>
    Task<IReadOnlyList<Tool>> ListVisibleToAsync(
        int userId, ToolVisibility? visibility, ToolStatus? status, bool mineOnly, CancellationToken ct);

    /// <summary>Like FindTrackedByIdAsync, but also finds soft-deleted tools. Used to restore.</summary>
    Task<Tool?> FindTrackedIncludingDeletedAsync(int toolId, CancellationToken ct);

    /// <summary>The owner's soft-deleted tools, with Owner loaded.</summary>
    Task<IReadOnlyList<Tool>> ListHiddenOwnedByAsync(int ownerId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);

}
