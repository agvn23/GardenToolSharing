namespace GardenToolSharing.Api.Application.Interfaces;

public record LenderTotal(int OwnerId, string OwnerName, int LoanCount);

public interface ILeaderboardRepository
{
    /// <summary>Tool owners ranked by total number of loans across all their tools, highest first.</summary>
    Task<IReadOnlyList<LenderTotal>> GetTopLendersAsync(int limit, CancellationToken ct);
}
