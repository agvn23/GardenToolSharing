using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class NoticeRepository(AppDbContext db) : INoticeRepository
{
    public async Task<IReadOnlyList<NoticeListItem>> ListAsync(int limit, CancellationToken ct) =>
        await db.Notices
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(limit)
            .Select(n => new NoticeListItem(
                n.Id, n.Title, n.Body, n.AuthorId, n.Author.DisplayName, n.CreatedAt, n.Comments.Count))
            .ToListAsync(ct);

    public Task<Notice?> FindWithCommentsAsync(int id, CancellationToken ct) =>
        db.Notices
            .AsNoTracking()
            .Include(n => n.Author)
            .Include(n => n.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public Task<Notice?> FindAsync(int id, CancellationToken ct) =>
        db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(Notice notice, CancellationToken ct)
    {
        db.Notices.Add(notice);
        await db.SaveChangesAsync(ct);
    }

    public void Remove(Notice notice) => db.Notices.Remove(notice);

    public Task<NoticeComment?> FindCommentAsync(int noticeId, int commentId, CancellationToken ct) =>
        db.NoticeComments
            .Include(c => c.Author)
            .Include(c => c.Notice)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.NoticeId == noticeId, ct);

    public async Task AddCommentAsync(NoticeComment comment, CancellationToken ct)
    {
        db.NoticeComments.Add(comment);
        await db.SaveChangesAsync(ct);
    }

    public void RemoveComment(NoticeComment comment) => db.NoticeComments.Remove(comment);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}