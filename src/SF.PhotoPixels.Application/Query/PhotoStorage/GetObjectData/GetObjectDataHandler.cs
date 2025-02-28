using System.Security.Cryptography;
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

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;

public class GetObjectDataHandler : IQueryHandler<GetObjectDataRequest, QueryResponse<ObjectDataResponse>>, IDisposable, IAsyncDisposable
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly MemoryStream _memoryStream = new();
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly SystemConfig _systemConfig;

    public GetObjectDataHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IOptionsMonitor<SystemConfig> systemConfigOptions)
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

    public async ValueTask<QueryResponse<ObjectDataResponse>> Handle(GetObjectDataRequest request, CancellationToken cancellationToken)
    {
        var metadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.ObjectId && x.UserId == _executionContextAccessor.UserId, cancellationToken);

        if (metadata == null)
        {
            return new NotFound();
        }

        var thumbnailExtension = Constants.SupportedVideoFormats.Contains($".{metadata.Extension}") ? "png" : "webp";
        var photo = await LoadPhoto(metadata.GetThumbnailName(thumbnailExtension), cancellationToken);

        await using var base64Stream = new CryptoStream(photo, new ToBase64Transform(), CryptoStreamMode.Read);
        using var streamReader = new StreamReader(base64Stream);

        return new ObjectDataResponse
        {
            Id = metadata.Id,
            Thumbnail = await streamReader.ReadToEndAsync(cancellationToken),
            ContentType = metadata.MimeType,
            Hash = metadata.Hash,
            AndroidCloudId = metadata.AndroidCloudId,
            AppleCloudId = metadata.AppleCloudId,
            Width = metadata.Width,
            Height = metadata.Height,
        };
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