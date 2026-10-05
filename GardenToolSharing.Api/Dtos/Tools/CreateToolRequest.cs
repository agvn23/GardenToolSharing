using System.ComponentModel.DataAnnotations;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Dtos.Tools;

public class CreateToolRequest : IValidatableObject
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    public ToolVisibility Visibility { get; init; } = ToolVisibility.Public;

    [Required]
    public DateOnly? AvailableFrom { get; init; }

    [Required]
    public DateOnly? AvailableUntil { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AvailableFrom is { } from && AvailableUntil is { } until && until < from)
        {
            yield return new ValidationResult(
                "Available until must be on or after available from.",
                [nameof(AvailableUntil)]);
        }
    }
}
