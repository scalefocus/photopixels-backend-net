using Microsoft.Extensions.Options;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class PhotoCreationHandler : IMediaCreationHandler
{
    private readonly IObjectStorage _objectStorage;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly SystemConfig _systemConfig;
    private readonly MemoryStream _memoryStream = new();

    public PhotoCreationHandler(IObjectStorage objectStorage,
        IExecutionContextAccessor executionContextAccessor,
        IOptions<SystemConfig> config
        )
    {
        _objectStorage = objectStorage;
        _executionContextAccessor = executionContextAccessor;
        _systemConfig = config.Value;
    }

    public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaCreationModel model, CancellationToken cancellationToken)
    {
        var photo = await LoadPhoto(model.FileName, cancellationToken);

        if (string.IsNullOrWhiteSpace(model.Format))
        {
            return new LoadMediaResponse
            {
                Media = photo,
                ContentType = !_systemConfig.PrivacyTestMode ? model.MimeType! : "image/jpeg",
            };
        }

        var formattedImage = await FormattedImage.LoadAsync(photo, cancellationToken);
        var ms = new MemoryStream();
        await formattedImage.SaveToFormat(ms, model.Format, cancellationToken);

        ms.Seek(0, SeekOrigin.Begin);

        return new LoadMediaResponse
        {
            Media = ms,
            ContentType = formattedImage.GetMimeType() ?? "",
        };
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
