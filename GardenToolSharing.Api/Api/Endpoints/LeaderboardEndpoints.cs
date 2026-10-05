using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Dtos.Leaderboard;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class LeaderboardEndpoints
{
    public static IEndpointRouteBuilder MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/leaderboard", async (
                string? period, ILeaderboardService leaderboard, CancellationToken ct) =>
                Results.Ok(await leaderboard.GetAsync(period, ct)))
            .WithTags("Leaderboard")
            .RequireAuthorization()
            .Produces<IReadOnlyList<LeaderboardEntryDto>>()
            .ProducesValidationProblem();

        return app;
    }
}
