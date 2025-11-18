using FFMpegCore;
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

            var tempInputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(model.FileName));

            await using (var file = File.Create(tempInputPath))
            {
                await videoStream.CopyToAsync(file, cancellationToken); //keep temporary file
            }

            var analysis = await FFProbe.AnalyseAsync(tempInputPath, cancellationToken: cancellationToken);
            var codec = analysis.VideoStreams.FirstOrDefault()?.CodecName?.ToLowerInvariant();

            string finalPath = tempInputPath;
            string contentType = model.MimeType;

            if (string.Equals(codec, "hevc", StringComparison.OrdinalIgnoreCase))
            {
                //TODO Remove  DateTime.Now.Ticks. it is there just to have unique file name
                var outputFileName = Path.GetFileNameWithoutExtension(model.FileName) + DateTime.Now.Ticks +  "-preview.mp4";
                var tempOutputPath = Path.Combine(Path.GetTempPath(), outputFileName);

                finalPath = await FormattedVideo.ConvertHevcVideoAsync(tempInputPath, tempOutputPath, cancellationToken);
                contentType = "video/mp4";
            }

            var finalStream = File.OpenRead(finalPath);

            return new LoadMediaResponse
            {
                Media = finalStream,
                ContentType = contentType
            };
        }
    }
}
