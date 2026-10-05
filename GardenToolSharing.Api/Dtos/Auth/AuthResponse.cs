using GardenToolSharing.Api.Dtos.Users;

namespace GardenToolSharing.Api.Dtos.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
