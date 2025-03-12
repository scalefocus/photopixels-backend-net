using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService
{
    public interface IVideoService
    {
        Task<long> SaveFile(RawVideo rawVideo, Guid userId, CancellationToken cancellationToken);

        Task<long> StoreObjectCreatedEventAsync(RawVideo rawVideo, long usedQuota, string filename, Guid userId, CancellationToken cancellationToken, string? AppleCloudId = null, string? AndroidCloudId = null);
    }
}