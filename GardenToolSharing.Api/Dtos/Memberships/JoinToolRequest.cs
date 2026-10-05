using System.ComponentModel.DataAnnotations;

namespace GardenToolSharing.Api.Dtos.Memberships;

public class JoinToolRequest
{
    [Required]
    public int? ToolId { get; init; }
}
