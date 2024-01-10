using Marten;
using Mediator;
using Microsoft.Extensions.Options;
using OneOf.Types;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadThumbnail;

public class LoadThumbnailHandler : IQueryHandler<LoadThumbnailRequest, QueryResponse<Stream>>, IDisposable, IAsyncDisposable
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly MemoryStream _memoryStream = new();
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly SystemConfig _systemConfig;

    public LoadThumbnailHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IOptionsMonitor<SystemConfig> systemConfigOptions)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _systemConfig = systemConfigOptions.CurrentValue;
    }

    public async ValueTask DisposeAsync()
    {
        await _session.DisposeAsync();
        await _memoryStream.DisposeAsync();
    }

    public void Dispose()
    {
        _session.Dispose();
        _memoryStream.Dispose();
    }

    public async ValueTask<QueryResponse<Stream>> Handle(LoadThumbnailRequest request, CancellationToken cancellationToken)
    {
        var metadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.ObjectId && x.UserId == _executionContextAccessor.UserId, cancellationToken);

        if (metadata == null)
        {
            return new NotFound();
        }

        var thumbnail = await LoadPhoto(metadata.GetThumbnailName(), cancellationToken);


        var formattedImage = await FormattedImage.LoadAsync(thumbnail, cancellationToken);
        var ms = new MemoryStream();
        await formattedImage.SaveAsync(_memoryStream, cancellationToken);

        _memoryStream.Seek(0, SeekOrigin.Begin);

        return _memoryStream;
    }

    private async Task<Stream> LoadPhoto(string name, CancellationToken cancellationToken)
    {
        if (!_systemConfig.PrivacyTestMode)
        {
            return await _objectStorage.LoadThumbnailAsync(_executionContextAccessor.UserId, name, cancellationToken);
        }

        if (_memoryStream.Length == 0)
        {
            PrivacyModeFileLoader.LoadThumbnailImage(_memoryStream);
        }

        _memoryStream.Seek(0, SeekOrigin.Begin);

        return _memoryStream;
    }
}