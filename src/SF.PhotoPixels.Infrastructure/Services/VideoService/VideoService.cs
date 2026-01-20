using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;
using Wolverine;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService;

public class VideoService : IVideoService
{
    private readonly IObjectRepository _objectRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly IMessageBus _bus;

    private const string ThumbnailExtension = "png";

    public VideoService(
        IObjectStorage objectStorage,
        IObjectRepository objectRepository, 
        IMessageBus bus
        )
    {
        _objectStorage = objectStorage;
        _objectRepository = objectRepository;
        _bus = bus;
    }

    public async Task<long> SaveFile(RawVideo rawVideo, Guid userId, CancellationToken cancellationToken)
    {
        var fingerprint = await rawVideo.GetSafeFingerprintAsync();
        var name = $"{fingerprint}{Path.GetExtension(rawVideo.GetFileName())}";
        var thumbnailName = $"{fingerprint}.{ThumbnailExtension}";

        await SaveVideoAsync(userId, name, rawVideo, cancellationToken);
        var thumbnailSize = await SaveThumbnailAsync(userId, name, thumbnailName, cancellationToken);
        return rawVideo.GetVideoSize() + thumbnailSize;
    }

    public async Task<long> StoreObjectCreatedEventAsync(RawVideo rawVideo, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null)
    {
        var fingerprint = await rawVideo.GetSafeFingerprintAsync();
        var hash = Convert.ToBase64String(await rawVideo.GetHashAsync());
        var objectId = new ObjectId(userId, fingerprint);
        var extension = FormattedVideo.GetExtension(rawVideo.GetFileName());

        var userFolders = _objectStorage.GetUserFolders(userId);
        var storedObjectPathName = Path.Combine(userFolders.ObjectFolder, $"{fingerprint}.{extension}");
        var video = await rawVideo.ToFormattedVideoAsync(storedObjectPathName, cancellationToken);

        //create video conversion command if necessery
        await _bus.PublishAsync(new ConvertVideoCommand { Path = storedObjectPathName });

        var evt = new MediaObjectCreated
        {
            ObjectId = objectId,
            Extension = extension,
            MimeType = video.GetMimeType(extension),
            Height = video.Height,
            Width = video.Width,
            Name = filename,
            Timestamp = new DateTimeOffset(video.GetDateTime()).ToUnixTimeMilliseconds(),
            Hash = fingerprint,
            OriginalHash = hash,
            UserId = userId,
            SizeInBytes = usedQuota,
            AppleCloudId = AppleCloudId,
            AndroidCloudId = AndroidCloudId,
        };

        //create video event projection
        return await _objectRepository.AddEvent(userId, evt, cancellationToken);
    }

    private Task SaveVideoAsync(Guid userId, string name, RawVideo rawVideo, CancellationToken cancellationToken)
    {
        return _objectStorage.StoreObjectAsync(userId, rawVideo, name, cancellationToken);
    }

    private async Task<long> SaveThumbnailAsync(Guid userId, string name, string thumbnailName, CancellationToken cancellationToken)
    {
        var userFolders = _objectStorage.GetUserFolders(userId);

        var objectFileName = Path.Combine(userFolders.ObjectFolder, name);
        var thumbnailFileName = Path.Combine(userFolders.ThumbFolder, thumbnailName);

        await FormattedVideo.SaveThumbnailAsync(objectFileName, thumbnailFileName, cancellationToken);

        var thumbnailFileInfo = new FileInfo(thumbnailFileName);
        return thumbnailFileInfo.Exists ? thumbnailFileInfo.Length : 0;
    }
}
