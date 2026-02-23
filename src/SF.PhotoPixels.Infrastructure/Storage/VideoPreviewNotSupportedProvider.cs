using FFMpegCore;
using System.Diagnostics;

namespace SF.PhotoPixels.Infrastructure.Storage;

/// <summary>
/// Singleton provider for generating and caching a minimal video with default font and "Preview not supported" text.
/// </summary>
public sealed class VideoPreviewNotSupportedProvider
{
    private static readonly Lazy<VideoPreviewNotSupportedProvider> _instance =
        new(() => new VideoPreviewNotSupportedProvider());

    private static readonly string _FfmpegExeFilePath = Path.Combine(GlobalFFOptions.Current.BinaryFolder, "ffmpeg");

    public static VideoPreviewNotSupportedProvider Instance => _instance.Value;

    private FileStream? _cachedStream;
    private string? _cachedFilePath;
    private readonly object _lock = new();

    private VideoPreviewNotSupportedProvider() { }

    /// <summary>
    /// Returns a FileStream containing the "Preview not supported" video.
    /// On first call, generates and stores the video; subsequent calls return the cached stream.
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

            _cachedFilePath ??= Path.Combine(Path.GetTempPath(), "video_preview_not_supported.mp4");

            if (!File.Exists(_cachedFilePath))
            {
                GeneratePreviewVideo(_cachedFilePath);
            }

            _cachedStream?.Dispose();
            _cachedStream = new FileStream(_cachedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _cachedStream.Position = 0;
            return _cachedStream;
        }
    }

    /// <summary>
    /// Generates a minimal MP4 video with "Preview not supported" text using ffmpeg and default font.
    /// </summary>
    private static void GeneratePreviewVideo(string outputPath)
    {
        var duration = 1; // seconds
        var width = 320;
        var height = 180;
        var fontSize = 24;

        // Use a simple color background and draw text in the center with default font
        var arguments =
            $"-y -f lavfi -i color=c=black:s={width}x{height}:d={duration} " +
            $"-vf \"drawtext=text='Preview not supported':fontcolor=white:fontsize={fontSize}:x=(w-text_w)/2:y=(h-text_h)/2\" " +
            $"-c:v libx264 -t {duration} -pix_fmt yuv420p \"{outputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _FfmpegExeFilePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            var eex = ex;
            throw;
        }        
        process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0 || !File.Exists(outputPath))
        {
            throw new InvalidOperationException("Failed to generate preview video with ffmpeg.");
        }
    }
}