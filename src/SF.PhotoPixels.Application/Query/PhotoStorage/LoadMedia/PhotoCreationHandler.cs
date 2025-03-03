using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia
{
    public class PhotoCreationHandler : IMediaCreationHandler, IDisposable, IAsyncDisposable
    {
        private readonly IObjectStorage _objectStorage;
        private readonly IExecutionContextAccessor _executionContextAccessor;
        private readonly SystemConfig _systemConfig;
        private readonly LoadMediaRequest _request;
        private readonly MemoryStream _memoryStream = new();

        public PhotoCreationHandler(IObjectStorage objectStorage,
            IExecutionContextAccessor executionContextAccessor,
            SystemConfig systemConfig,
            LoadMediaRequest request)
        {
            _objectStorage = objectStorage;
            _executionContextAccessor = executionContextAccessor;
            _request = request;
            _systemConfig = systemConfig;
        }

        public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(ObjectProperties? metadata, CancellationToken cancellationToken)
        {
            var photo = await LoadPhoto(metadata.GetImageName(), cancellationToken);

            if (string.IsNullOrWhiteSpace(_request.Format))
            {
                return new LoadMediaResponse
                {
                    Media = photo,
                    ContentType = !_systemConfig.PrivacyTestMode ? metadata.MimeType! : "image/jpeg",
                };
            }

            var formattedImage = await FormattedImage.LoadAsync(photo, cancellationToken);
            var ms = new MemoryStream();
            await formattedImage.SaveToFormat(ms, _request.Format, cancellationToken);

            ms.Seek(0, SeekOrigin.Begin);

            return new LoadMediaResponse
            {
                Media = ms,
                ContentType = formattedImage.GetMimeType() ?? "",
            };
        }

        public void Dispose()
        {
            _memoryStream.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _memoryStream.DisposeAsync();
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
}
