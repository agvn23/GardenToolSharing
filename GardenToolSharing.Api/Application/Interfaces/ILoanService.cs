using GardenToolSharing.Api.Dtos.Loans;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface ILoanService
{
    Task<LoanDto> CreateAsync(int borrowerId, CreateLoanRequest request, CancellationToken ct);
    Task<LoanDto> UpdateAsync(int borrowerId, int loanId, UpdateLoanRequest request, CancellationToken ct);
    Task DeleteAsync(int borrowerId, int loanId, CancellationToken ct);

    /// <summary>Owner-only: closes the tool's active loan and frees the tool.</summary>
    Task<LoanDto> ReturnAsync(int ownerId, int toolId, CancellationToken ct);
}
