namespace GardenToolSharing.Api.Application.Interfaces;

public record LenderTotal(int OwnerId, string OwnerName, int LoanCount, int ActiveLoanCount);

public interface ILeaderboardRepository
{
    /// <summary>Tool owners ranked by loans currently out, then total loans ever made, highest first.</summary>
    Task<IReadOnlyList<LenderTotal>> GetTopLendersAsync(int limit, CancellationToken ct);
}
