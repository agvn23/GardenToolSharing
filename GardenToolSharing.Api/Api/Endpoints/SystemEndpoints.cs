using GardenToolSharing.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        // Liveness: the process is up and handling requests. Checks nothing else.
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithTags("System")
            .AllowAnonymous();

        // Readiness: the app can actually serve requests - DB reachable and migrations applied.
        app.MapGet("/ready", async (AppDbContext db, CancellationToken ct) =>
            {
                if (!await db.Database.CanConnectAsync(ct))
                {
                    return Results.Problem(
                        title: "Not ready",
                        detail: "Database is not reachable.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                var pending = await db.Database.GetPendingMigrationsAsync(ct);
                if (pending.Any())
                {
                    return Results.Problem(
                        title: "Not ready",
                        detail: "Database has pending migrations.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Ok(new { status = "ready" });
            })
            .WithTags("System")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
