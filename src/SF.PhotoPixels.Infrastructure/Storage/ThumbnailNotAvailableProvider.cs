using ImageMagick;
using System.Runtime.InteropServices;

namespace SF.PhotoPixels.Infrastructure.Storage;

/// <summary>
/// Singleton provider for generating and caching a "Thumbnail not available" image as a FileStream.
/// </summary>
public sealed class ThumbnailNotAvailableProvider
{
    private static readonly Lazy<ThumbnailNotAvailableProvider> _instance =
        new(() => new ThumbnailNotAvailableProvider());

    public static ThumbnailNotAvailableProvider Instance => _instance.Value;

    private FileStream? _cachedStream;
    private string? _cachedFilePath;
    private readonly object _lock = new();

    private ThumbnailNotAvailableProvider() { }

    /// <summary>
    /// Returns a FileStream containing the "Thumbnail not available" image.
    /// On first call, generates and stores the image; subsequent calls return the cached stream.
    /// The stream is kept open for reuse. Caller should not dispose the stream.
    /// </summary>
    public FileStream Get()
    {
        lock (_lock)
        {
            if (_cachedStream != null && _cachedStream.CanRead)
            {
                _cachedStream.Position = 0;
                return _cachedStream;
            }

            // Create and cache the thumbnail file
            _cachedFilePath ??= Path.Combine(Path.GetTempPath(), "thumbnail_not_available.webp");

            if (!File.Exists(_cachedFilePath))
            {
                using (var placeholder = new MagickImage(MagickColors.White, 160, 160))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        SetLinuxConfiguration(placeholder);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        SetWindowsConfiguration(placeholder);
                    // else unknown config - Draw picture without label 

                    // Draw a discrete gray frame with 2px margin
                    var frameColor = new MagickColor("#CCCCCC"); // light gray
                    int margin = 2;
                    int frameWidth = (int)placeholder.Width - 2 * margin;
                    int frameHeight = (int)placeholder.Height - 4 * margin;
                    placeholder.Draw(new ImageMagick.Drawing.Drawables()
                        .StrokeColor(frameColor)
                        .StrokeWidth(1)
                        .FillColor(MagickColors.None)
                        .Rectangle(margin, margin, margin + frameWidth - 1, margin + frameHeight - 1)
                    );

                    placeholder.Format = MagickFormat.WebP;
                    placeholder.Write(_cachedFilePath);
                }
            }

            _cachedStream?.Dispose();
            _cachedStream = new FileStream(_cachedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _cachedStream.Position = 0;
            return _cachedStream;
        }
    }

    private static void SetWindowsConfiguration(MagickImage placeholder)
    {
        placeholder.Settings.FontPointsize = 16;
        placeholder.Settings.FillColor = MagickColors.Black;
        placeholder.Settings.TextGravity = Gravity.Center;
        placeholder.Annotate("Thumb not available", Gravity.Center);
    }

    private static void SetLinuxConfiguration(MagickImage placeholder)
    {
        placeholder.Settings.FontPointsize = 12;
        placeholder.Settings.FillColor = MagickColors.Black;
        placeholder.Settings.TextGravity = Gravity.Center;
        if (File.Exists("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"))
        {
            placeholder.Settings.Font = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
            placeholder.Annotate("Thumb not available", Gravity.Center);
        }
    }
}