using System.ComponentModel.DataAnnotations;

namespace GardenToolSharing.Api.Dtos.Auth;

public class LoginRequest
{
    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
