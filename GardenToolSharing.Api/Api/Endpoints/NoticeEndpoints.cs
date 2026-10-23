using System.Security.Claims;
using GardenToolSharing.Api.Api.Filters;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Dtos.Notices;

namespace GardenToolSharing.Api.Api.Endpoints;

public static class NoticeEndpoints
{
    public static IEndpointRouteBuilder MapNoticeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notices").WithTags("Notices").RequireAuthorization();

        group.MapGet("/", async (INoticeService notices, CancellationToken ct) =>
                Results.Ok(await notices.ListAsync(ct)))
            .Produces<IReadOnlyList<NoticeDto>>();

        group.MapGet("/{id:int}", async (int id, INoticeService notices, CancellationToken ct) =>
                Results.Ok(await notices.GetAsync(id, ct)))
            .Produces<NoticeDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
                CreateNoticeRequest request, ClaimsPrincipal user, INoticeService notices, CancellationToken ct) =>
            {
                var notice = await notices.CreateAsync(user.GetUserId(), request, ct);
                return Results.Created($"/notices/{notice.Id}", notice);
            })
            .AddEndpointFilter<ValidationFilter<CreateNoticeRequest>>()
            .RequireRateLimiting("default-write")
            .Produces<NoticeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", async (
                int id, ClaimsPrincipal user, INoticeService notices, CancellationToken ct) =>
            {
                await notices.DeleteAsync(user.GetUserId(), id, ct);
                return Results.NoContent();
            })
            .RequireRateLimiting("default-write")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/comments", async (
                int id, CreateCommentRequest request, ClaimsPrincipal user, INoticeService notices, CancellationToken ct) =>
            {
                var comment = await notices.AddCommentAsync(user.GetUserId(), id, request, ct);
                return Results.Created($"/notices/{id}", comment);
            })
            .AddEndpointFilter<ValidationFilter<CreateCommentRequest>>()
            .RequireRateLimiting("default-write")
            .Produces<NoticeCommentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}/comments/{commentId:int}", async (
                int id, int commentId, ClaimsPrincipal user, INoticeService notices, CancellationToken ct) =>
            {
                await notices.DeleteCommentAsync(user.GetUserId(), id, commentId, ct);
                return Results.NoContent();
            })
            .RequireRateLimiting("default-write")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}