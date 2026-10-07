using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Leaderboard;

namespace GardenToolSharing.Api.Application.Services;

public class LeaderboardService(ILeaderboardRepository repository) : ILeaderboardService
{
    private const int Limit = 10;

    // "total" is the only period defined in the brief; kept as an explicit, validated value
    // rather than silently ignored, so a typo in the query string doesn't return the wrong data.
    private static readonly HashSet<string> SupportedPeriods = new(StringComparer.OrdinalIgnoreCase) { "total" };

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetAsync(string? period, CancellationToken ct)
    {
        if (period is not null && !SupportedPeriods.Contains(period))
            throw DomainValidationException.ForField("period", "Only 'total' is currently supported.");

        var rows = await repository.GetTopLendersAsync(Limit, ct);

        return rows
            .Select((r, index) => new LeaderboardEntryDto(index + 1, r.OwnerId, r.OwnerName, r.LoanCount, r.ActiveLoanCount))
            .ToList();
    }
}
