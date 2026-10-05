using System.Security.Claims;
using GardenToolSharing.Api.Api.Filters;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Dtos.Memberships;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class MembershipEndpoints
{
    public static IEndpointRouteBuilder MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/memberships").WithTags("Memberships").RequireAuthorization();

        group.MapPost("/", async (
                JoinToolRequest request, ClaimsPrincipal user, IMembershipService memberships, CancellationToken ct) =>
            {
                var membership = await memberships.JoinAsync(user.GetUserId(), request, ct);
                return Results.Created($"/memberships/{membership.Id}", membership);
            })
            .AddEndpointFilter<ValidationFilter<JoinToolRequest>>()
            .RequireRateLimiting("default-write")
            .Produces<MembershipDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Owner approves a pending (private-tool) request. No body: PATCH here always means "approve".
        group.MapPatch("/{id:int}", async (
                int id, ClaimsPrincipal user, IMembershipService memberships, CancellationToken ct) =>
                Results.Ok(await memberships.ApproveAsync(user.GetUserId(), id, ct)))
            .Produces<MembershipDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (
                int id, ClaimsPrincipal user, IMembershipService memberships, CancellationToken ct) =>
            {
                await memberships.LeaveAsync(user.GetUserId(), id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
