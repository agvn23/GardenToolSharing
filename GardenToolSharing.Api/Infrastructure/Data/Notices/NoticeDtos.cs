using System.ComponentModel.DataAnnotations;

namespace GardenToolSharing.Api.Dtos.Notices;

public record NoticeCommentDto(int Id, int AuthorId, string AuthorName, string Body, DateTime CreatedAt);

/// <summary>Comments is null in the list view and filled in on the single-notice view.</summary>
public record NoticeDto(
    int Id,
    string Title,
    string Body,
    int AuthorId,
    string AuthorName,
    DateTime CreatedAt,
    int CommentCount,
    IReadOnlyList<NoticeCommentDto>? Comments = null);

public class CreateNoticeRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Body { get; init; } = string.Empty;
}

public class CreateCommentRequest
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Body { get; init; } = string.Empty;
}