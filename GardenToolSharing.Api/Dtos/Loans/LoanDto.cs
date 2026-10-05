namespace GardenToolSharing.Api.Dtos.Loans;

public record LoanDto(
    int Id,
    int ToolId,
    string ToolName,
    int BorrowerId,
    string BorrowerName,
    DateOnly BorrowedFrom,
    DateOnly BorrowedUntil,
    DateTime? ReturnedAt,
    string? Note,
    DateTime CreatedAt,
    bool IsActive);
