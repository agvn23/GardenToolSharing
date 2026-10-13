namespace GardenToolSharing.Api.Application.Interfaces;

public interface IPhotoStorage
{
    Task SaveAsync(string storedFileName, Stream content, CancellationToken ct);
    Stream? Open(string storedFileName);
    void Delete(string storedFileName);
}