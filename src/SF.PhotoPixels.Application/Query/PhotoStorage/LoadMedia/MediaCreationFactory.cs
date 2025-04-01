using Microsoft.Extensions.DependencyInjection;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class MediaCreationFactory : IMediaCreationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MediaCreationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMediaCreationHandler CreateMediaHandler(string extension)
    {
        return extension switch
        {
            var ext when Constants.SupportedVideoFormats.Contains($".{ext}") => _serviceProvider.GetRequiredKeyedService<IMediaCreationHandler>(MediaType.Video),
            var ext when Constants.SupportedPhotoFormats.Contains($".{ext}") => _serviceProvider.GetRequiredKeyedService<IMediaCreationHandler>(MediaType.Photo),
            _ => throw new NotSupportedException("Unsupported media type")
        };
    }
}

