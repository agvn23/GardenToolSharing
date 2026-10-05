namespace GardenToolSharing.Api.Dtos.Loans;

/// <summary>
/// Partial update. Only BorrowedUntil and Note can change; BorrowedFrom and the tool are fixed
/// once a loan exists. Note: because plain nullable strings can't distinguish "omitted" from
/// "explicitly cleared" in JSON, sending "note": null and omitting "note" behave the same (no change).
/// </summary>
public class UpdateLoanRequest
{
    public DateOnly? BorrowedUntil { get; init; }
    public string? Note { get; init; }
}
