using System.Security.Cryptography;
using Marten;
using Mediator;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectDataList;

public class GetObjectDataListHandler : IRequestHandler<GetObjectDataListRequest, OneOf<IList<ObjectDataResponse>, NotFound, ValidationError>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly SystemConfig _systemConfig;
    private MemoryStream? _memoryStream;

    public GetObjectDataListHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IOptionsMonitor<SystemConfig> systemConfigOptions)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _systemConfig = systemConfigOptions.CurrentValue;
    }

    public async ValueTask<OneOf<IList<ObjectDataResponse>, NotFound, ValidationError>> Handle(GetObjectDataListRequest request, CancellationToken cancellationToken)
    {
        if (request.ObjectIds.Count >= 100)
        {
            return new ValidationError("BatchLimitReached", "Not permited to request more objects than batch limit");
        }

        var objectProperties = await _session.Query<ObjectProperties>()
            .Where(x => request.ObjectIds.Contains(x.Id) && x.UserId == _executionContextAccessor.UserId)
            .ToListAsync(cancellationToken);

        var response = new List<ObjectDataResponse>();

        if (!objectProperties.Any())
        {
            return response;
        }

        foreach (var obj in objectProperties)
        {
            _memoryStream = new MemoryStream();

            try
            {
                var thumbnailExtension = Constants.SupportedVideoFormats.Contains($".{obj.Extension}") ? "png" : "webp";
                var photo = await LoadPhoto(obj.GetThumbnailName(thumbnailExtension), cancellationToken);

                await using var base64Stream = new CryptoStream(photo, new ToBase64Transform(), CryptoStreamMode.Read);
                using var streamReader = new StreamReader(base64Stream);

                var thumbnail = new ObjectDataResponse
                {
                    Id = obj.Id,
                    Thumbnail = await streamReader.ReadToEndAsync(cancellationToken),
                    ContentType = obj.MimeType,
                    Hash = obj.Hash,
                    AndroidCloudId = obj.AndroidCloudId,
                    AppleCloudId = obj.AppleCloudId,
                    Width = obj.Width,
                    Height = obj.Height,
                };

                response.Add(thumbnail);
            }
            finally
            {
                await _memoryStream.DisposeAsync();
            }
        }

        return response;
    }

    private async Task<Stream> LoadPhoto(string name, CancellationToken cancellationToken)
    {
        if (!_systemConfig.PrivacyTestMode)
        {
            return await _objectStorage.LoadThumbnailAsync(_executionContextAccessor.UserId, name, cancellationToken);
        }

        if (_memoryStream!.Length == 0)
        {
            PrivacyModeFileLoader.LoadThumbnailImage(_memoryStream);
        }

        _memoryStream.Seek(0, SeekOrigin.Begin);

        return _memoryStream;
    }
}