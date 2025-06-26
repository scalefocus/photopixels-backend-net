using System.Drawing;
using System.Globalization;
using FFMpegCore;
using SF.PhotoPixels.Domain.Utils;

namespace SF.PhotoPixels.Infrastructure.Storage;

public sealed class FormattedVideo : IDisposable, IStorageItem
{
    private readonly MediaFormat _format;
    private readonly IMediaAnalysis _video;

    public int Width => _video.PrimaryVideoStream?.Width ?? 0;
    public int Height => _video.PrimaryVideoStream?.Height ?? 0;

    private FormattedVideo(IMediaAnalysis video, MediaFormat format)
    {
        _video = video;
        _format = format;
    }

    public void Dispose()
    {
    }

    public async static Task<FormattedVideo> LoadAsync(Stream source, CancellationToken cancellationToken = default)
    {
        var video = await FFProbe.AnalyseAsync(source);
        return new FormattedVideo(video, video.Format);
    }

    public string GetMimeType(string extension)
    {
        return MimeTypes.GetMimeTypeFromExtension(extension);
    }

    public string GetExtension(string filename)
    {
        return Path.GetExtension(filename).TrimStart('.');
    }

    public string GetName(string filename)
    {
        return Path.GetFileNameWithoutExtension(filename);
    }

    public async Task<bool> SaveThumbnailAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        await FFMpeg.SnapshotAsync(inputPath, outputPath, new Size(200, -1), TimeSpan.FromSeconds(1));
        return true;
    }

    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public DateTime GetDateTime()
    {
        if (_format.Tags != null && _format.Tags.TryGetValue("creation_time", out var tag) &&
            DateTime.TryParse(tag, CultureInfo.InvariantCulture, out DateTime parsedDateTime))
        {
            return parsedDateTime;
        }

        return DateTime.UtcNow;
    }
}