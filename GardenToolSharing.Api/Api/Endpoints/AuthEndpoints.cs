using System.Security.Claims;
using GardenToolSharing.Api.Api.Filters;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Dtos.Auth;
using GardenToolSharing.Api.Dtos.Users;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService auth, CancellationToken ct) =>
            {
                var response = await auth.RegisterAsync(request, ct);
                return Results.Created("/auth/me", response);
            })
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
            .AllowAnonymous()
            .RequireRateLimiting("default-write")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/login", async (LoginRequest request, IAuthService auth, CancellationToken ct) =>
                Results.Ok(await auth.LoginAsync(request, ct)))
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .AllowAnonymous()
            .RequireRateLimiting("default-write")
            .Produces<AuthResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", async (ClaimsPrincipal principal, IAuthService auth, CancellationToken ct) =>
                Results.Ok(await auth.GetCurrentUserAsync(principal.GetUserId(), ct)))
            .RequireAuthorization()
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
