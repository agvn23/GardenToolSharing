using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Auth;
using GardenToolSharing.Api.Dtos.Users;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    ITokenService tokens,
    TimeProvider time) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();

        if (await users.EmailExistsAsync(email, ct))
            throw DomainValidationException.ForField("email", "Email is already registered.");

        var user = new User
        {
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            CreatedAt = time.GetUtcNow().UtcDateTime
        };

        await users.AddAsync(user, ct);
        return ToResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim(), ct);

        // Same message for unknown email and wrong password, so accounts can't be probed
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return ToResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId, ct)
                   ?? throw new UnauthorizedException("User no longer exists.");

        return ToDto(user);
    }

    private AuthResponse ToResponse(User user)
    {
        var token = tokens.CreateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAt, ToDto(user));
    }

    private static UserDto ToDto(User user) => new(user.Id, user.DisplayName, user.Email);
}
