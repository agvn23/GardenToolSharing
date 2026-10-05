using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public record TokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    TokenResult CreateToken(User user);
}
