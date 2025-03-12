using Marten;
using Mediator;
using OneOf;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Services.PhotoService;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;

public class StorePhotoHandler : IRequestHandler<StorePhotoRequest, OneOf<StorePhotoResponse, Duplicate, ValidationError>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly IPhotoService _photoService;

    public StorePhotoHandler(
        IDocumentSession session,
        IExecutionContextAccessor executionContextAccessor,
        IPhotoService photoService)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _photoService = photoService;
    }

    public async ValueTask<OneOf<StorePhotoResponse, Duplicate, ValidationError>> Handle(StorePhotoRequest request, CancellationToken cancellationToken)
    {
        using var rawImage = new RawImage(request.File.OpenReadStream());


        var imageHash = Convert.ToBase64String(await rawImage.GetHashAsync());
        if (imageHash != request.ObjectHash)
        {
            return new ValidationError(nameof(request.ObjectHash), "Object hash does not match");
        }

        var imageFingerprint = await rawImage.GetSafeFingerprintAsync();
        var objectId = new ObjectId(_executionContextAccessor.UserId, imageFingerprint);
        if (await _session.Query<ObjectProperties>().AnyAsync(x => x.Id == objectId, cancellationToken))
        {
            return new Duplicate();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);
        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var usedQuota = await _photoService.SaveFile(rawImage, user.Id, cancellationToken);
        if (!user.IncreaseUsedQuota(usedQuota))
        {
            return new ValidationError("QuotaReached", "User quota is reached, not enough capacity for new upload");
        }

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        var filename = request.File.FileName;

        var version = await _photoService.StoreObjectCreatedEventAsync(rawImage, usedQuota, filename, user.Id, cancellationToken, request.AppleCloudId, request.AndroidCloudId);

        return new StorePhotoResponse
        {
            Id = objectId,
            Revision = version,
            Quota = user.Quota,
            UsedQuota = user.UsedQuota
        };
    }
}