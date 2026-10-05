namespace GardenToolSharing.Api.Common;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Per-user for authenticated requests (JWT "sub" claim), per-IP for anonymous ones
    /// (register/login, where there's no user yet). Used to partition rate limiter buckets.
    /// </summary>
    public static string GetRateLimitPartitionKey(this HttpContext context)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        return userId is not null
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress}";
    }
}
