using FFMpegCore;
using SF.PhotoPixels.Domain.Utils;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace SF.PhotoPixels.Infrastructure.Storage;

public sealed class FormattedVideo : IDisposable, IStorageItem
{
    private readonly MediaFormat? _format;
    private readonly IMediaAnalysis? _video;

    public int Width => _video?.PrimaryVideoStream?.Width ?? 0;
    public int Height => _video?.PrimaryVideoStream?.Height ?? 0;

    private static readonly string _FfmpegExeFilePath = Path.Combine(GlobalFFOptions.Current.BinaryFolder, "ffmpeg");

    private FormattedVideo(IMediaAnalysis? video, MediaFormat? format) // Updated parameters to nullable types
    {
        _video = video;
        _format = format;
    }

    public void Dispose()
    {
    }

    public async static Task<FormattedVideo> LoadAsync(string source, CancellationToken cancellationToken = default)
    {
        var video = await FFProbe.AnalyseAsync(source);
        return new FormattedVideo(video, video.Format);
    }

    public string GetMimeType(string extension)
    {
        return MimeTypes.GetMimeTypeFromExtension(extension);
    }

    public static string GetExtension(string filename)
    {
        return Path.GetExtension(filename).TrimStart('.');
    }

    public static string GetName(string filename)
    {
        return Path.GetFileNameWithoutExtension(filename);
    }

    public static async Task<bool> SaveThumbnailAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            await FFMpeg.SnapshotAsync(inputPath, outputPath, new Size(200, -1), TimeSpan.FromSeconds(1));
        }
        catch (Exception)
        {
            await CreateThumbnailUsingFfmpeg(inputPath, outputPath, new ThumbnailSize(200, 200, 1));
        }
        return true;
    }

    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public DateTime GetDateTime()
    {
        if (_format?.Tags != null && _format.Tags.TryGetValue("creation_time", out var tag) &&
            DateTime.TryParse(tag, CultureInfo.InvariantCulture, out DateTime parsedDateTime))
        {
            return parsedDateTime;
        }

        return DateTime.UtcNow;
    }

    static Task<long> CreateThumbnailUsingFfmpeg(string mediaFilePath, string outputPath, ThumbnailSize thumbnailSize)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        string additionalVideoParameters = "-ss 5 -an";
        var tcs = new TaskCompletionSource<long>();

        var process = new Process
        {
            StartInfo =
                    {
                        FileName = _FfmpegExeFilePath,
                        Arguments = $"{additionalVideoParameters} -i \"{mediaFilePath}\" -vframes 1 -s {thumbnailSize.Width}x{thumbnailSize.Height} -y \"{outputPath}\""
                    },
            EnableRaisingEvents = true
        };

        process.Exited += (sender, args) =>
        {
            process.Dispose();
            stopwatch.Stop();
            tcs.SetResult(stopwatch.ElapsedMilliseconds);
        };
        process.Start();
        return tcs.Task;
    }

    public static Task<string> ConvertHevcVideoAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(outputPath))
        {
            return Task.FromResult(outputPath);
        }

        var tcs = new TaskCompletionSource<string>();

        var process = new Process
        {
            StartInfo =
            {
                FileName = _FfmpegExeFilePath,
                Arguments = $"-y -i \"{inputPath}\" -c:v libx264 -c:a aac -movflags +faststart \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.Exited += (_, _) =>
        {
            try
            {
                if (process.ExitCode == 0)
                {
                    tcs.TrySetResult(outputPath);
                }
                else
                {
                    tcs.TrySetException(
                        new InvalidOperationException($"FFmpeg exited with code {process.ExitCode}."));
                }
            }
            finally
            {
                process.Dispose();
            }
        };

        if (!process.Start())
        {
            tcs.SetException(new InvalidOperationException("Failed to start ffmpeg process."));
        }

        process.BeginErrorReadLine();

        if (cancellationToken != CancellationToken.None)
        {
            cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch
                {
                    // ignore
                }

                tcs.TrySetCanceled(cancellationToken);
            });
        }

        return tcs.Task;
    }


    private class ThumbnailSize
    {
        public ThumbnailSize(uint originalWidth, uint originalHeight, uint sizeDivider)
        {
            Max = Math.Max(originalWidth, originalHeight) / sizeDivider;
            Width = originalWidth / sizeDivider;
            Height = originalHeight / sizeDivider;
        }
        public uint Width;
        public uint Height;
        public uint Max;
    }
}
