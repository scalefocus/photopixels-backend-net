using System.Text;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Infrastructure.Services.PhotoService;

public class PhotoService : IPhotoService
{
    private readonly IObjectRepository _objectRepository;
    private readonly IObjectStorage _objectStorage;

    public PhotoService(
        IObjectStorage objectStorage,
        IObjectRepository objectRepository)
    {
        _objectStorage = objectStorage;
        _objectRepository = objectRepository;
    }

    public async Task<long> SaveFile(RawImage rawImage, Guid userId, CancellationToken cancellationToken)
    {
        var fingerprint = await rawImage.GetSafeFingerprintAsync();
        var image = await rawImage.ToFormattedImageAsync(cancellationToken);

        var saveImage = SaveImageAsync(userId, fingerprint, image, rawImage, cancellationToken);
        var saveThumbnail = SaveThumbnailAsync(userId, fingerprint, image, cancellationToken);

        await Task.WhenAll(saveImage, saveThumbnail);

        return rawImage.GetImageSize() + saveThumbnail.Result;
    }

    public async Task<long> StoreObjectCreatedEventAsync(RawImage rawImage, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null)
    {
        var fingerprint = await rawImage.GetSafeFingerprintAsync();
        var hash = Convert.ToBase64String(await rawImage.GetHashAsync());
        var objectId = new ObjectId(userId, fingerprint);
        var image = await rawImage.ToFormattedImageAsync(cancellationToken);

        var evt = new MediaObjectCreated
        {
            ObjectId = objectId,
            Extension = image.GetExtension(),
            MimeType = image.GetMimeType(),
            Height = image.Height,
            Width = image.Width,
            Name = filename,
            Timestamp = image.GetDateTime().ToUnixTimeMilliseconds(),
            Hash = fingerprint,
            OriginalHash = hash,
            UserId = userId,
            SizeInBytes = usedQuota,
            AppleCloudId = AppleCloudId,
            AndroidCloudId = AndroidCloudId,
        };

        return await _objectRepository.AddEvent(userId, evt, cancellationToken);
    }

    private Task SaveImageAsync(Guid userId, string fingerprint, FormattedImage image, RawImage rawImage, CancellationToken cancellationToken)
    {
        var name = $"{fingerprint}.{image.GetExtension()}";

        return _objectStorage.StoreObjectAsync(userId, rawImage, name, cancellationToken);
    }

    private Task<long> SaveThumbnailAsync(Guid userId, string fingerprint, FormattedImage image, CancellationToken cancellationToken)
    {
        using var imageThumbnail = image.GetThumbnail();
        var thumbnailName = $"{fingerprint}.{imageThumbnail.GetExtension()}";

        return _objectStorage.StoreThumbnailAsync(userId, imageThumbnail, thumbnailName, cancellationToken);
    }
}
