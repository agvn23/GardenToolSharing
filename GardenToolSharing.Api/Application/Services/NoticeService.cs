using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Notices;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class NoticeService(INoticeRepository notices, TimeProvider time) : INoticeService
{
    private const int ListLimit = 50;

    public async Task<IReadOnlyList<NoticeDto>> ListAsync(CancellationToken ct)
    {
        var items = await notices.ListAsync(ListLimit, ct);
        return items
            .Select(i => new NoticeDto(i.Id, i.Title, i.Body, i.AuthorId, i.AuthorName, i.CreatedAt, i.CommentCount))
            .ToList();
    }

    public async Task<NoticeDto> GetAsync(int id, CancellationToken ct)
    {
        var notice = await notices.FindWithCommentsAsync(id, ct)
                     ?? throw new NotFoundException("Notice not found.");
        return ToDto(notice);
    }

    public async Task<NoticeDto> CreateAsync(int authorId, CreateNoticeRequest request, CancellationToken ct)
    {
        var title = request.Title.Trim();
        var body = request.Body.Trim();

        if (title.Length < 2)
            throw DomainValidationException.ForField("title", "Title must be at least 2 characters.");
        if (body.Length == 0)
            throw DomainValidationException.ForField("body", "Write something in the notice.");

        var notice = new Notice
        {
            AuthorId = authorId,
            Title = title,
            Body = body,
            CreatedAt = time.GetUtcNow().UtcDateTime
        };
        await notices.AddAsync(notice, ct);

        var created = await notices.FindWithCommentsAsync(notice.Id, ct)
                      ?? throw new InvalidOperationException("Created notice could not be reloaded.");
        return ToDto(created);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken ct)
    {
        var notice = await notices.FindAsync(id, ct)
                     ?? throw new NotFoundException("Notice not found.");

        if (notice.AuthorId != userId)
            throw new ForbiddenException("Only the author can delete a notice.");

        notices.Remove(notice);
        await notices.SaveChangesAsync(ct);
    }

    public async Task<NoticeCommentDto> AddCommentAsync(
        int authorId, int noticeId, CreateCommentRequest request, CancellationToken ct)
    {
        _ = await notices.FindAsync(noticeId, ct) ?? throw new NotFoundException("Notice not found.");

        var body = request.Body.Trim();
        if (body.Length == 0)
            throw DomainValidationException.ForField("body", "Write something in the comment.");

        var comment = new NoticeComment
        {
            NoticeId = noticeId,
            AuthorId = authorId,
            Body = body,
            CreatedAt = time.GetUtcNow().UtcDateTime
        };
        await notices.AddCommentAsync(comment, ct);

        var created = await notices.FindCommentAsync(noticeId, comment.Id, ct)
                      ?? throw new InvalidOperationException("Created comment could not be reloaded.");
        return ToCommentDto(created);
    }

    public async Task DeleteCommentAsync(int userId, int noticeId, int commentId, CancellationToken ct)
    {
        var comment = await notices.FindCommentAsync(noticeId, commentId, ct)
                      ?? throw new NotFoundException("Comment not found.");

        // The comment's author, or the author of the notice it sits under
        if (comment.AuthorId != userId && comment.Notice.AuthorId != userId)
            throw new ForbiddenException("You can't delete this comment.");

        notices.RemoveComment(comment);
        await notices.SaveChangesAsync(ct);
    }

    private static NoticeDto ToDto(Notice n) => new(
        n.Id, n.Title, n.Body, n.AuthorId, n.Author.DisplayName, n.CreatedAt, n.Comments.Count,
        n.Comments
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
            .Select(c => ToCommentDto(c))
            .ToList());

    private static NoticeCommentDto ToCommentDto(NoticeComment c) =>
        new(c.Id, c.AuthorId, c.Author.DisplayName, c.Body, c.CreatedAt);
}