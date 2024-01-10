namespace SF.PhotoPixels.Application.PrivacyMode;

public static class PrivacyModeFileLoader
{
    public static void LoadFullImage(Stream responseStream)
    {
        using var stream = typeof(PrivacyModeFileLoader).Assembly
            .GetManifestResourceStream($"{typeof(PrivacyModeFileLoader).Namespace}.fullimage.jpeg");

        if (stream == null)
        {
            throw new Exception("Full image not found");
        }

        stream.CopyTo(responseStream);
    }

    public static void LoadThumbnailImage(Stream responseStream)
    {
        using var stream = typeof(PrivacyModeFileLoader).Assembly
            .GetManifestResourceStream($"{typeof(PrivacyModeFileLoader).Namespace}.thumbnail.webp");

        if (stream == null)
        {
            throw new Exception("Thumbnail image not found");
        }

        stream.CopyTo(responseStream);
    }
}