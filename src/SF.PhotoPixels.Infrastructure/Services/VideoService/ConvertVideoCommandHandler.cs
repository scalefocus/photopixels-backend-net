using FFMpegCore;
using Marten;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Storage;
using SF.PhotoPixels.Infrastructure.Stores;
using System.Runtime.InteropServices;
using Wolverine.Attributes;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService;

[LocalQueue("EncodeVideoQueue")]
public class ConvertVideoCommandHandler
{
    private const string ThumbnailExtension = "png";

    public async Task Handle(ConvertVideoCommand command, IDocumentSession _session)
    {
        // create preview video and save to object storage
        var videoFile = Path.Combine(command.objectPath, command.Filename); //replace with actual path from object storage
        var tempInputPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            "/tmp" :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sf-photos", "temp");
        var tempFile = Path.Combine(tempInputPath, command.Filename); //replace with actual path from object storage

        File.Copy(videoFile, tempFile, true);

        var analysis = await FFProbe.AnalyseAsync(tempFile, cancellationToken: CancellationToken.None);
        var codec = analysis.VideoStreams.FirstOrDefault()?.CodecName?.ToLowerInvariant();
        if (codec == null || !string.Equals(codec, "hevc", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var outputFileName = $"{Path.GetFileNameWithoutExtension(command.Filename)}{Constants.PreviewSufix}.mp4";
        var convertedOutputFile = Path.Combine(command.convertedVideoPath, outputFileName);

        await FormattedVideo.ConvertHevcVideoAsync(tempFile, convertedOutputFile, CancellationToken.None);

        // create thumbnail and save to object storage, then delete the original video from object storage
        var thumbnailName = $"{Path.GetFileNameWithoutExtension(command.Filename)}.{ThumbnailExtension}";
        var thumbnailFilePath = Path.Combine(command.thumbVideoPath, thumbnailName);

        await FormattedVideo.SaveThumbnailAsync(convertedOutputFile, thumbnailFilePath, CancellationToken.None);

        // increase user quota
        var size = new FileInfo(convertedOutputFile).Length;
        var user = await _session.LoadAsync<Domain.Entities.User>(command.UserId);
        if (user != null)
        {
            user.IncreaseUsedQuota(size);
            _session.Update(user);
            await _session.SaveChangesAsync(CancellationToken.None);
            
            if (user.UsedQuota > user.Quota)
                UserCancellationStore.AddUser(user.Id);
        }

        // delete the temp video from object temp storage
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }
}
