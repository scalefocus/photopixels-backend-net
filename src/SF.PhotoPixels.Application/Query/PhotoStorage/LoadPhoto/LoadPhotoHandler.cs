using Marten;
using Mediator;
using Microsoft.Extensions.Options;
using OneOf.Types;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadPhoto;

public class LoadPhotoHandler : IQueryHandler<LoadPhotoRequest, QueryResponse<PhotoResponse>>, IDisposable, IAsyncDisposable
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly MemoryStream _memoryStream = new();
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly SystemConfig _systemConfig;

    public LoadPhotoHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IOptionsMonitor<SystemConfig> systemConfigOptions)
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

    public async ValueTask<QueryResponse<PhotoResponse>> Handle(LoadPhotoRequest request, CancellationToken cancellationToken)
    {
        var metadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == _executionContextAccessor.UserId, cancellationToken);

        if (metadata == null)
        {
            return new NotFound();
        }

        if (Constants.SupportedVideoFormats.Contains($".{metadata.Extension}"))
        {
            var videoStream = await _objectStorage.LoadObjectAsync(_executionContextAccessor.UserId, metadata.GetImageName(), cancellationToken);      

            return new PhotoResponse
            {
                Photo = videoStream,
                ContentType = metadata.MimeType,
            };
        } 
        else
        {
            var photo = await LoadPhoto(metadata.GetImageName(), cancellationToken);

            if (string.IsNullOrWhiteSpace(request.Format))
            {
                return new PhotoResponse
                {
                    Photo = photo,
                    ContentType = !_systemConfig.PrivacyTestMode ? metadata.MimeType! : "image/jpeg",
                };
            }

            var formattedImage = await FormattedImage.LoadAsync(photo, cancellationToken);
            var ms = new MemoryStream();
            await formattedImage.SaveToFormat(ms, request.Format, cancellationToken);

            ms.Seek(0, SeekOrigin.Begin);

            return new PhotoResponse
            {
                Photo = ms,
                ContentType = formattedImage.GetMimeType() ?? "",
            };
        }        
    }

    private async Task<Stream> LoadPhoto(string name, CancellationToken cancellationToken)
    {
        if (!_systemConfig.PrivacyTestMode)
        {
            return await _objectStorage.LoadObjectAsync(_executionContextAccessor.UserId, name, cancellationToken);
        }

        if (_memoryStream.Length == 0)
        {
            PrivacyModeFileLoader.LoadFullImage(_memoryStream);
        }

        _memoryStream.Seek(0, SeekOrigin.Begin);

        return _memoryStream;
    }
}