using GardenToolSharing.Api.Dtos.Leaderboard;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardEntryDto>> GetAsync(string? period, CancellationToken ct);
}
