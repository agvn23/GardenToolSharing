using System.Security.Claims;
using GardenToolSharing.Api.Api.Filters;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Dtos.Loans;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/loans").WithTags("Loans").RequireAuthorization();

        group.MapPost("/", async (
                CreateLoanRequest request, ClaimsPrincipal user, ILoanService loans, CancellationToken ct) =>
            {
                var loan = await loans.CreateAsync(user.GetUserId(), request, ct);
                return Results.Created($"/loans/{loan.Id}", loan);
            })
            .AddEndpointFilter<ValidationFilter<CreateLoanRequest>>()
            .RequireRateLimiting("loan-write")
            .Produces<LoanDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}", async (
                int id, UpdateLoanRequest request, ClaimsPrincipal user, ILoanService loans, CancellationToken ct) =>
                Results.Ok(await loans.UpdateAsync(user.GetUserId(), id, request, ct)))
            .Produces<LoanDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (
                int id, ClaimsPrincipal user, ILoanService loans, CancellationToken ct) =>
            {
                await loans.DeleteAsync(user.GetUserId(), id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
