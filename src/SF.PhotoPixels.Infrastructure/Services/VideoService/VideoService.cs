using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService;

public class VideoService : IVideoService
{
    private readonly IObjectRepository _objectRepository;
    private readonly IObjectStorage _objectStorage;

    private const string ThumbnailExtension = "png";

    public VideoService(
        IObjectStorage objectStorage,
        IObjectRepository objectRepository)
    {
        _objectStorage = objectStorage;
        _objectRepository = objectRepository;
    }

    public async Task<long> SaveFile(RawVideo rawVideo, Guid userId, CancellationToken cancellationToken)
    {
        var fingerprint = await rawVideo.GetSafeFingerprintAsync();
        var video = await rawVideo.ToFormattedVideoAsync(cancellationToken);
        var name = $"{fingerprint}{Path.GetExtension(rawVideo.GetFileName())}";
        var thumbnailName = $"{fingerprint}.{ThumbnailExtension}";

        await SaveVideoAsync(userId, name, rawVideo, cancellationToken);
        var thumbnailSize = await SaveThumbnailAsync(userId, name, thumbnailName, video, cancellationToken);
        return rawVideo.GetVideoSize() + thumbnailSize;
    }

    public async Task<long> StoreObjectCreatedEventAsync(RawVideo rawVideo, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null)
    {
        var fingerprint = await rawVideo.GetSafeFingerprintAsync();
        var hash = Convert.ToBase64String(await rawVideo.GetHashAsync());
        var objectId = new ObjectId(userId, fingerprint);
        var video = await rawVideo.ToFormattedVideoAsync(cancellationToken);
        var extension = video.GetExtension(filename);

        var evt = new MediaObjectCreated
        {
            ObjectId = objectId,
            Extension = video.GetExtension(filename),
            MimeType = video.GetMimeType(extension),
            Height = video.Height,
            Width = video.Width,
            Name = video.GetName(filename),
            Timestamp = new DateTimeOffset(video.GetDateTime()).ToUnixTimeMilliseconds(),
            Hash = fingerprint,
            OriginalHash = hash,
            UserId = userId,
            SizeInBytes = usedQuota,
            AppleCloudId = AppleCloudId,
            AndroidCloudId = AndroidCloudId,
        };

        return await _objectRepository.AddEvent(userId, evt, cancellationToken);
    }

    private Task SaveVideoAsync(Guid userId, string name, RawVideo rawVideo, CancellationToken cancellationToken)
    {
        return _objectStorage.StoreObjectAsync(userId, rawVideo, name, cancellationToken);
    }

    private async Task<long> SaveThumbnailAsync(Guid userId, string name, string thumbnailName, FormattedVideo video, CancellationToken cancellationToken)
    {
        var userFolders = _objectStorage.GetUserFolders(userId);

        var objectFileName = Path.Combine(userFolders.ObjectFolder, name);
        var thumbnailFileName = Path.Combine(userFolders.ThumbFolder, thumbnailName);

        await video.SaveThumbnailAsync(objectFileName, thumbnailFileName, cancellationToken);

        var thumbnailFileInfo = new FileInfo(thumbnailFileName);
        return thumbnailFileInfo.Exists ? thumbnailFileInfo.Length : 0;
    }
}
