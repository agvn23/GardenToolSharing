using System.Security.Claims;
using GardenToolSharing.Api.Api.Filters;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Dtos.Loans;
using GardenToolSharing.Api.Dtos.Tools;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class ToolEndpoints
{
    public static IEndpointRouteBuilder MapToolEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tools").WithTags("Tools").RequireAuthorization();

        group.MapPost("/", async (
                CreateToolRequest request, ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
            {
                var tool = await tools.CreateAsync(user.GetUserId(), request, ct);
                return Results.Created($"/tools/{tool.Id}", tool);
            })
            .AddEndpointFilter<ValidationFilter<CreateToolRequest>>()
            .RequireRateLimiting("default-write")
            .Produces<ToolDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", async (
                ToolVisibility? visibility, ToolStatus? status, bool? mine,
                ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
                Results.Ok(await tools.ListAsync(user.GetUserId(), visibility, status, mine ?? false, ct)))
            .Produces<IReadOnlyList<ToolDto>>();

        group.MapGet("/{id:int}", async (
                int id, ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
                Results.Ok(await tools.GetAsync(user.GetUserId(), id, ct)))
            .Produces<ToolDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Owner-only: closes the tool's active loan and frees the tool for the next borrower.
        group.MapPost("/{id:int}:return", async (
                int id, ClaimsPrincipal user, ILoanService loans, CancellationToken ct) =>
                Results.Ok(await loans.ReturnAsync(user.GetUserId(), id, ct)))
            .RequireRateLimiting("default-write")
            .Produces<LoanDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
