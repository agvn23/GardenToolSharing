using System.Security.Claims;
using GardenToolSharing.Api.Common.Exceptions;

namespace GardenToolSharing.Api.Common;

public static class UserClaimsExtensions
{
    /// <summary>Reads the user id from the JWT "sub" claim (or its mapped NameIdentifier form).</summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst("sub")?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedException("Invalid or missing user identity.");
    }
}
