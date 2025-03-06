using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia
{
    public class VideoCreationHandler : IMediaCreationHandler
    {
        private readonly IObjectStorage _objectStorage;
        private readonly IExecutionContextAccessor _executionContextAccessor;

        public VideoCreationHandler(IObjectStorage objectStorage,
            IExecutionContextAccessor executionContextAccessor)
        {
            _objectStorage = objectStorage;
            _executionContextAccessor = executionContextAccessor;
        }

        public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaCreationModel model, CancellationToken cancellationToken)
        {
            var videoStream = await _objectStorage.LoadObjectAsync(_executionContextAccessor.UserId, model.FileName, cancellationToken);

            return new LoadMediaResponse
            {
                Media = videoStream,
                ContentType = model.MimeType,
            };
        }
    }
}
