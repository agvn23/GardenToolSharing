using System.ComponentModel.DataAnnotations;

namespace GardenToolSharing.Api.Dtos.Loans;

public class CreateLoanRequest
{
    [Required]
    public int? ToolId { get; init; }

    /// <summary>Defaults to today (UTC) if omitted.</summary>
    public DateOnly? BorrowedFrom { get; init; }

    [Required]
    public DateOnly? BorrowedUntil { get; init; }

    [StringLength(500)]
    public string? Note { get; init; }
}
