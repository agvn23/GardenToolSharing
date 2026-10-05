using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface ILoanRepository
{
    /// <summary>Adds the loan and saves. Any pending change on a tracked Tool (e.g. Status) in the
    /// same DbContext is persisted in the same call, since services mutate the tracked Tool first.</summary>
    Task AddAsync(Loan loan, CancellationToken ct);

    /// <summary>Loan by id, tracked, with Tool and Borrower loaded (for owner/borrower checks and mutation).</summary>
    Task<Loan?> FindByIdAsync(int id, CancellationToken ct);

    /// <summary>The current active (not yet returned) loan for a tool, tracked, or null.</summary>
    Task<Loan?> FindActiveByToolIdAsync(int toolId, CancellationToken ct);

    void Remove(Loan loan);

    Task SaveChangesAsync(CancellationToken ct);
}
