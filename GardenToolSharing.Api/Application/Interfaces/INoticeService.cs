using GardenToolSharing.Api.Dtos.Notices;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface INoticeService
{
    Task<IReadOnlyList<NoticeDto>> ListAsync(CancellationToken ct);
    Task<NoticeDto> GetAsync(int id, CancellationToken ct);
    Task<NoticeDto> CreateAsync(int authorId, CreateNoticeRequest request, CancellationToken ct);
    Task DeleteAsync(int userId, int id, CancellationToken ct);
    Task<NoticeCommentDto> AddCommentAsync(int authorId, int noticeId, CreateCommentRequest request, CancellationToken ct);
    Task DeleteCommentAsync(int userId, int noticeId, int commentId, CancellationToken ct);
}