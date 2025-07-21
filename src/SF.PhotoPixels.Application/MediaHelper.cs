using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure;

namespace SF.PhotoPixels.Application;

public static class MediaHelper
{
    public static string GetMediaType(string extension)
    {
        return extension switch
        {
            var ext when Constants.SupportedVideoFormats.Contains($".{ext}") => MediaType.Video.ToString().ToLower(),
            var ext when Constants.SupportedPhotoFormats.Contains($".{ext}") => MediaType.Photo.ToString().ToLower(),
            _ => MediaType.Unknown.ToString().ToLower()
        };
    }
}
