using Marten;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia
{
    public static class MediaCreationFactory
    {
        public static IMediaCreationHandler CreateMediaHandler(ObjectProperties? metadata, LoadMediaRequest request, IExecutionContextAccessor executionContextAccessor, IObjectStorage objectStorage, IDocumentSession session, SystemConfig systemConfig)
        {
            return metadata.Extension switch
            {
                var ext when Constants.SupportedVideoFormats.Contains($".{ext}") => new VideoCreationHandler(objectStorage, executionContextAccessor) as IMediaCreationHandler,
                var ext when Constants.SupportedPhotoFormats.Contains($".{ext}") => new PhotoCreationHandler(objectStorage, executionContextAccessor, systemConfig, request) as IMediaCreationHandler,
                _ => throw new NotSupportedException("Unsupported media type")
            };
        }
    }
}
