using GardenToolSharing.Api.Dtos.Auth;
using GardenToolSharing.Api.Dtos.Users;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<UserDto> GetCurrentUserAsync(int userId, CancellationToken ct);
}
