namespace GardenToolSharing.Api.Dtos.Tools;

public record MyLoanDto(int Id, DateOnly BorrowedFrom, DateOnly BorrowedUntil, string? Note, DateTime EditableUntil);