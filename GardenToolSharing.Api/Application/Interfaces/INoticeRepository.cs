using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public record NoticeListItem(
    int Id, string Title, string Body, int AuthorId, string AuthorName, DateTime CreatedAt, int CommentCount);

public interface INoticeRepository
{
    /// <summary>Newest notices first, with author name and comment count, no comment bodies.</summary>
    Task<IReadOnlyList<NoticeListItem>> ListAsync(int limit, CancellationToken ct);

    /// <summary>One notice with its author and all comments (and their authors), untracked.</summary>
    Task<Notice?> FindWithCommentsAsync(int id, CancellationToken ct);

    /// <summary>Tracked, for deleting.</summary>
    Task<Notice?> FindAsync(int id, CancellationToken ct);

    Task AddAsync(Notice notice, CancellationToken ct);
    void Remove(Notice notice);

    /// <summary>Tracked, with Author and Notice loaded.</summary>
    Task<NoticeComment?> FindCommentAsync(int noticeId, int commentId, CancellationToken ct);

    Task AddCommentAsync(NoticeComment comment, CancellationToken ct);
    void RemoveComment(NoticeComment comment);
    Task SaveChangesAsync(CancellationToken ct);
}