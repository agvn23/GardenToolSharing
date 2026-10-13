using GardenToolSharing.Api.Application.Interfaces;

namespace GardenToolSharing.Api.Infrastructure.Storage;

public class DiskPhotoStorage : IPhotoStorage
{
    private readonly string _root;

    public DiskPhotoStorage(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["Uploads:Path"];
        _root = string.IsNullOrWhiteSpace(configured) ? Path.Combine(env.ContentRootPath, "uploads") : configured;
        Directory.CreateDirectory(_root);
    }

    // GetFileName strips any directory parts, so a stored name can never escape the folder
    private string PathFor(string storedFileName) => Path.Combine(_root, Path.GetFileName(storedFileName));

    public async Task SaveAsync(string storedFileName, Stream content, CancellationToken ct)
    {
        await using var file = File.Create(PathFor(storedFileName));
        await content.CopyToAsync(file, ct);
    }

    public Stream? Open(string storedFileName)
    {
        var path = PathFor(storedFileName);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    public void Delete(string storedFileName)
    {
        var path = PathFor(storedFileName);
        if (File.Exists(path)) File.Delete(path);
    }
}