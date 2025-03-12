using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using SF.PhotoPixels.Domain.Entities;
using SolidTUS.Handlers;
using SolidTUS.Models;

namespace SF.PhotoPixels.Infrastructure.BackgroundServices;

public class TusExpirationHandler : IExpiredUploadHandler
{

    private readonly IServiceProvider _provider;
    private readonly ISystemClock _clock;
    private readonly IUploadMetaHandler _uploadMetaHandler;
    private readonly IUploadStorageHandler _uploadStorageHandler;

    public TusExpirationHandler(
        IServiceProvider provider,
        ISystemClock clock,
        IUploadMetaHandler uploadMetaHandler,
        IUploadStorageHandler uploadStorageHandler)
    {
        _provider = provider;
        _clock = clock;
        _uploadMetaHandler = uploadMetaHandler;
        _uploadStorageHandler = uploadStorageHandler;

    }

    public async Task ExpiredUploadAsync(UploadFileInfo uploadFileInfo, CancellationToken cancellationToken)
    {
        var fileSize = long.Parse(uploadFileInfo.Metadata["fileSize"]!);
        var userId = Guid.Parse(uploadFileInfo.Metadata["userId"]);

        await using (var scope = _provider.CreateAsyncScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
            var user = await session.LoadAsync<User>(userId);

            user.DecreaseUsedQuota(fileSize);

            session.Update(user);
            await session.SaveChangesAsync(cancellationToken);

            await _uploadMetaHandler.DeleteUploadFileInfoAsync(uploadFileInfo, cancellationToken);
            await _uploadStorageHandler.DeleteFileAsync(uploadFileInfo, cancellationToken);
        }
    }

    public async Task StartScanForExpiredUploadsAsync(CancellationToken cancellationToken)
    {
        await foreach (var info in _uploadMetaHandler.GetAllResourcesAsync())
        {
            if (info.ExpirationDate.HasValue && !info.Done)
            {
                var now = _clock.UtcNow;
                var expired = now > info.ExpirationDate.Value;
                if (expired)
                {
                    await ExpiredUploadAsync(info, cancellationToken);
                }
            }
        }
    }
}
