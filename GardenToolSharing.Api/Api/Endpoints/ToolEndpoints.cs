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

            group.MapGet("/hidden", async (ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
        Results.Ok(await tools.ListHiddenAsync(user.GetUserId(), ct)))
    .Produces<IReadOnlyList<ToolDto>>();

group.MapPut("/{id:int}", async (
        int id, CreateToolRequest request, ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
        Results.Ok(await tools.UpdateAsync(user.GetUserId(), id, request, ct)))
    .AddEndpointFilter<ValidationFilter<CreateToolRequest>>()
    .RequireRateLimiting("default-write")
    .Produces<ToolDto>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict);

group.MapDelete("/{id:int}", async (
        int id, ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
    {
        await tools.DeleteAsync(user.GetUserId(), id, ct);
        return Results.NoContent();
    })
    .RequireRateLimiting("default-write")
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict);

group.MapPost("/{id:int}:restore", async (
        int id, ClaimsPrincipal user, IToolService tools, CancellationToken ct) =>
        Results.Ok(await tools.RestoreAsync(user.GetUserId(), id, ct)))
    .RequireRateLimiting("default-write")
    .Produces<ToolDto>()
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
