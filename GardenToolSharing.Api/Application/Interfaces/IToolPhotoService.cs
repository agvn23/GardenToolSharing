using GardenToolSharing.Api.Dtos.Tools;

namespace GardenToolSharing.Api.Application.Interfaces;

public record PhotoFile(Stream Content, string ContentType);

public interface IToolPhotoService
{
    Task<ToolPhotoDto> AddAsync(int ownerId, int toolId, string? caption, Stream content, long length, CancellationToken ct);
    Task DeleteAsync(int ownerId, int toolId, int photoId, CancellationToken ct);
    Task<PhotoFile> GetAsync(int viewerId, int toolId, int photoId, CancellationToken ct);
}