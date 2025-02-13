using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace SF.PhotoPixels.Infrastructure.Storage;

public class LocalObjectStorage : IObjectStorage
{
    private readonly ILogger<LocalObjectStorage> _logger;
    private const string ThumbDirectory = "thumbnails";
    private readonly string _imagesDirectory = Path.Combine(GetRootDirectory(), "sf-photos", "images");

    public LocalObjectStorage(ILogger<LocalObjectStorage> logger)
    {
        _logger = logger;

        _logger.LogDebug("Images directory: {ImagesDirectory}", _imagesDirectory);
    }

    public ValueTask<FileStream> LoadObjectAsync(Guid userId, string imageName, CancellationToken cancellationToken = new())
    {
        return new ValueTask<FileStream>(File.OpenRead(Path.Combine(_imagesDirectory, userId.ToString(), imageName)));
    }

    public ValueTask<FileStream> LoadThumbnailAsync(Guid userId, string imageName, CancellationToken cancellationToken = new())
    {
        var thumbDirectory = Path.Combine(_imagesDirectory, userId.ToString(), ThumbDirectory, imageName);

        return new ValueTask<FileStream>(File.OpenRead(thumbDirectory));
    }

    public async Task StoreObjectAsync(Guid userId, IStorageItem item, string name, CancellationToken cancellationToken = new())
    {
        EnsureUserFolders(userId);

        var path = Path.Combine(_imagesDirectory, userId.ToString(), name);
        await StoreObjectInternal(path, item, cancellationToken);
    }

    public async Task<long> StoreThumbnailAsync(Guid userId, IStorageItem item, string name, CancellationToken cancellationToken = new())
    {
        EnsureUserFolders(userId);

        var path = Path.Combine(_imagesDirectory, userId.ToString(), ThumbDirectory, name);

         return await StoreObjectInternal(path, item, cancellationToken);
    }

    public bool DeleteObject(Guid userId, string name)
    {
        var path = Path.Combine(_imagesDirectory, userId.ToString(), name);

        if (File.Exists(Path.Combine(path)))
        {
            _logger.LogDebug("Deleting photo: {name}", name);

            File.Delete(path);
            return true;
        }
        return false;
    }

    public bool DeleteThumbnail(Guid userId,  string name)
    {
        var path = Path.Combine(_imagesDirectory, userId.ToString(), ThumbDirectory, name);

        if (File.Exists(Path.Combine(path)))
        {
            _logger.LogDebug("Deleting thumbnail: {name}", name);

            File.Delete(path);
            return true;
        }
        return false;
    }

    private static string GetRootDirectory()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine(Path.DirectorySeparatorChar + "var", "data") : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private void EnsureUserFolders(Guid userId)
    {
        var userDirectory = Path.Combine(_imagesDirectory, userId.ToString());

        if (!Directory.Exists(Path.Combine(userDirectory)))
        {
            _logger.LogDebug("Creating user directory: {UserDirectory}", userDirectory);

            Directory.CreateDirectory(userDirectory);
        }

        var thumbDirectory = Path.Combine(userDirectory, ThumbDirectory);

        if (!Directory.Exists(thumbDirectory))
        {
            _logger.LogDebug("Creating user thumbnail directory: {UserDirectory}", thumbDirectory);

            Directory.CreateDirectory(Path.Combine(userDirectory, ThumbDirectory));
        }
    }

    private static async Task<long> StoreObjectInternal(string path, IStorageItem item, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenWrite(path);
        await item.SaveAsync(stream, cancellationToken);
        return stream.Length;
    }

    public void DeleteUserFolders(Guid userId)
    {
        var userDirectory = Path.Combine(_imagesDirectory, userId.ToString());

        if (Directory.Exists(Path.Combine(userDirectory)))
        {
            _logger.LogDebug("Deleting user directory: {UserDirectory}", userDirectory);

            Directory.Delete(userDirectory, true);
        }
    }
}