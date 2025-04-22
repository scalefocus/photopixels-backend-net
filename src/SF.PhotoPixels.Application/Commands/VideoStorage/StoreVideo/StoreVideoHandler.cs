using Marten;
using Mediator;
using OneOf;
using SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Services.VideoService;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;

public class StoreVideoHandler : IRequestHandler<StoreVideoRequest, OneOf<IMediaResponse, Duplicate, ValidationError>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly IVideoService _videoService;

    public StoreVideoHandler(
        IDocumentSession session,
        IExecutionContextAccessor executionContextAccessor,
        IVideoService videoService)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _videoService = videoService;
    }

    public async ValueTask<OneOf<IMediaResponse, Duplicate, ValidationError>> Handle(StoreVideoRequest request, CancellationToken cancellationToken)
    {
        using var rawVideo = new RawVideo(request.File.OpenReadStream(), request.File.FileName);

        var videoOriginalHash = Convert.ToBase64String(await rawVideo.GetHashAsync());
        if (videoOriginalHash != request.ObjectHash)
        {
            return new ValidationError(nameof(request.ObjectHash), "Object hash does not match");
        }

        var videoFingerprint = await rawVideo.GetSafeFingerprintAsync();
        var objectId = new ObjectId(_executionContextAccessor.UserId, videoFingerprint);
        if (await _session.Query<ObjectProperties>().AnyAsync(x => x.Id == objectId, cancellationToken))
        {
            return new Duplicate();
        }

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);
        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var usedQuota = await _videoService.SaveFile(rawVideo, user.Id, cancellationToken);
        if (!user.IncreaseUsedQuota(usedQuota))
        {
            return new ValidationError("QuotaReached", "User quota is reached, not enough capacity for new upload");
        }

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        var filename = request.File.FileName;

        var version = await _videoService.StoreObjectCreatedEventAsync(rawVideo, usedQuota, filename, user.Id, cancellationToken, request.AppleCloudId, request.AndroidCloudId);

        return new StoreVideoResponse
        {
            Id = objectId,
            Revision = version,
            Quota = user.Quota,
            UsedQuota = user.UsedQuota
        };
    }
}