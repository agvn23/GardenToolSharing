using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Tools;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class ToolPhotoService(
    IToolRepository tools,
    IToolPhotoRepository photos,
    IPhotoStorage storage,
    TimeProvider time) : IToolPhotoService
{
    private const int MaxPhotosPerTool = 6;
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<ToolPhotoDto> AddAsync(
        int ownerId, int toolId, string? caption, Stream content, long length, CancellationToken ct)
    {
        await RequireOwnedIdleToolAsync(ownerId, toolId, ct);

        if (length <= 0 || length > MaxBytes)
            throw DomainValidationException.ForField("file", "A photo must be no larger than 5 MB.");

        var cleanCaption = string.IsNullOrWhiteSpace(caption) ? "Photo" : caption.Trim();
        if (cleanCaption.Length > 100)
            throw DomainValidationException.ForField("caption", "Caption can be at most 100 characters.");

        if (await photos.CountForToolAsync(toolId, ct) >= MaxPhotosPerTool)
            throw new ConflictException($"A tool can have at most {MaxPhotosPerTool} photos.");

        var detected = await DetectImageTypeAsync(content, ct)
            ?? throw DomainValidationException.ForField("file", "Only JPEG, PNG and WebP images are accepted.");

        var storedName = $"{Guid.NewGuid():N}{detected.Extension}";
        await storage.SaveAsync(storedName, content, ct);

        var photo = new ToolPhoto
        {
            ToolId = toolId,
            Caption = cleanCaption,
            StoredFileName = storedName,
            ContentType = detected.ContentType,
            CreatedAt = time.GetUtcNow().UtcDateTime
        };

        try
        {
            await photos.AddAsync(photo, ct);
        }
        catch
        {
            storage.Delete(storedName); // don't leave an orphaned file behind
            throw;
        }

        return new ToolPhotoDto(photo.Id, photo.Caption, photo.CreatedAt);
    }

    public async Task DeleteAsync(int ownerId, int toolId, int photoId, CancellationToken ct)
    {
        await RequireOwnedIdleToolAsync(ownerId, toolId, ct);

        var photo = await photos.FindAsync(toolId, photoId, ct)
                    ?? throw new NotFoundException("Photo not found.");

        photos.Remove(photo);
        await photos.SaveChangesAsync(ct);
        storage.Delete(photo.StoredFileName);
    }

    public async Task<PhotoFile> GetAsync(int viewerId, int toolId, int photoId, CancellationToken ct)
    {
        // Any logged-in user may see a photo, but a removed tool's photos are for its owner only
        var tool = await tools.FindTrackedIncludingDeletedAsync(toolId, ct);
        if (tool is null || (tool.IsDeleted && tool.OwnerId != viewerId))
            throw new NotFoundException("Photo not found.");

        var photo = await photos.FindAsync(toolId, photoId, ct)
                    ?? throw new NotFoundException("Photo not found.");

        var stream = storage.Open(photo.StoredFileName)
                     ?? throw new NotFoundException("Photo not found.");

        return new PhotoFile(stream, photo.ContentType);
    }

    private async Task RequireOwnedIdleToolAsync(int ownerId, int toolId, CancellationToken ct)
    {
        var tool = await tools.FindTrackedByIdAsync(toolId, ct)
                   ?? throw new NotFoundException("Tool not found.");

        if (tool.OwnerId != ownerId)
            throw new ForbiddenException("Only the tool's owner can change its photos.");

        if (tool.Status != ToolStatus.Available)
            throw new ConflictException("Photos can't be changed while the tool is lent out.");
    }

    // Judge the file by its first bytes, never by the name or content type the client claims
    private static async Task<(string ContentType, string Extension)?> DetectImageTypeAsync(
        Stream content, CancellationToken ct)
    {
        var header = new byte[12];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        content.Position = 0;

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ("image/jpeg", ".jpg");

        if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return ("image/png", ".png");

        if (read >= 12 && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
            && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
            return ("image/webp", ".webp");

        return null;
    }
}