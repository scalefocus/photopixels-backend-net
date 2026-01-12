using System.Net;
using System.Runtime.InteropServices;
using JasperFx.Core;
using Marten;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Exceptions;
using SF.PhotoPixels.Infrastructure.Services.PhotoService;
using SF.PhotoPixels.Infrastructure.Services.VideoService;
using SF.PhotoPixels.Infrastructure.Storage;
using SolidTUS.Handlers;
using SolidTUS.Models;

namespace SF.PhotoPixels.Infrastructure.Services.TusService;

public class TusService : ITusService
{
    private readonly IDocumentSession _session;
    private readonly IPhotoService _photoService;
    private readonly IVideoService _videoService;

    private readonly IUploadMetaHandler _uploadMetaHandler;
    private readonly IUploadStorageHandler _uploadStorageHandler;

    private static readonly IReadOnlyCollection<string> RequiredMetadata = new List<string>() { "fileExtension", "fileName", "fileHash", "fileSize", "userId" };
    private static readonly IReadOnlyCollection<string> OptionalValueMetadata = new List<string>() { "appleId", "androidId" };

    public TusService(
        IDocumentSession session,
        IPhotoService photoService,
        IUploadMetaHandler uploadMetaHandler,
        IUploadStorageHandler uploadStorageHandler,
        IVideoService videoService)
    {
        _session = session;
        _photoService = photoService;
        _uploadMetaHandler = uploadMetaHandler;
        _uploadStorageHandler = uploadStorageHandler;
        _videoService = videoService;
    }

    public async Task HandleNewCompletion(UploadFileInfo fileInfo)
    {
        var ctx = new CancellationTokenSource();
        var extension = fileInfo.Metadata!["fileExtension"];
        using var fs = await SaveCompletedFileWithExtensionNew(Path.Combine(fileInfo.OnDiskDirectoryPath!, fileInfo.OnDiskFilename), extension);

        if (Constants.SupportedVideoFormats.Contains($".{extension}"))
        {
            using RawVideo rawVideo = await SaveVideo(fileInfo, ctx, fs);
        }
        else
        {
            using RawImage rawImage = await SavePhoto(fileInfo, ctx, fs);
        }

        await _uploadMetaHandler.DeleteUploadFileInfoAsync(fileInfo, ctx.Token);
        await _uploadStorageHandler.DeleteFileAsync(fileInfo, ctx.Token);
        File.Delete($"{Path.Combine(fileInfo.OnDiskDirectoryPath!, fileInfo.OnDiskFilename)}.{extension}");
    }

    private async Task<RawImage> SavePhoto(UploadFileInfo fileInfo, CancellationTokenSource ctx, FileStream fs)
    {
        var rawImage = new RawImage(fs, fileInfo.Metadata!["fileName"]);

        var imageHash = Convert.ToBase64String(await rawImage.GetHashAsync());
        if (!imageHash.Equals(fileInfo.Metadata!["fileHash"]))
        {
            throw new FailRequestException("Object hash does not match", HttpStatusCode.BadRequest);
        }

        var userId = Guid.Parse(fileInfo.Metadata!["userId"]);
        var user = await _session.LoadAsync<User>(userId) ?? throw new FailRequestException("User not found", HttpStatusCode.BadRequest);

        var usedQuota = await _photoService.SaveFile(rawImage, user.Id, ctx.Token);
        if (!user.IncreaseUsedQuota(usedQuota))
        {
            throw new FailRequestException("User quota is reached, not enough capacity for new upload", HttpStatusCode.BadRequest);
        }

        _session.Update(user);
        await _session.SaveChangesAsync(ctx.Token);

        var canGetApple = fileInfo.Metadata!.TryGetValue("appleId", out var apple);
        if (!canGetApple) apple = "";

        var canGetRobot = fileInfo.Metadata!.TryGetValue("androidId", out var robot);
        if (!canGetRobot) robot = "";

        await _photoService.StoreObjectCreatedEventAsync(rawImage, usedQuota, fileInfo.Metadata!["fileName"], user.Id, ctx.Token, apple, robot);
        fs.Close();
        return rawImage;
    }

    private async Task<RawVideo> SaveVideo(UploadFileInfo fileInfo, CancellationTokenSource ctx, FileStream fs)
    {
        var rawVideo = new RawVideo(fs, $"{fileInfo.OnDiskFilename}.{fileInfo.Metadata!["fileExtension"]}");
        var videoHash = Convert.ToBase64String(await rawVideo.GetHashAsync());
        if (!videoHash.Equals(fileInfo.Metadata["fileHash"]))
        {
            throw new FailRequestException("Object hash does not match", HttpStatusCode.BadRequest);
        }

        var userId = Guid.Parse(fileInfo.Metadata!["userId"]);
        var user = await _session.LoadAsync<User>(userId) ?? throw new FailRequestException("User not found", HttpStatusCode.BadRequest);

        var usedQuota = await _videoService.SaveFile(rawVideo, user.Id, ctx.Token);
        if (!user.IncreaseUsedQuota(usedQuota))
        {
            throw new FailRequestException("User quota is reached, not enough capacity for new upload", HttpStatusCode.BadRequest);
        }

        _session.Update(user);
        await _session.SaveChangesAsync(ctx.Token);

        var canGetApple = fileInfo.Metadata!.TryGetValue("appleId", out var apple);
        if (!canGetApple) apple = "";

        var canGetRobot = fileInfo.Metadata!.TryGetValue("androidId", out var robot);
        if (!canGetRobot) robot = "";

        await _videoService.StoreObjectCreatedEventAsync(rawVideo, usedQuota, fileInfo.Metadata!["fileName"], user.Id, ctx.Token, apple, robot);
        fs.Close();
        return rawVideo;
    }

    private static bool ValidateMetadataEntry(KeyValuePair<string, string> a)
    {
        if (!RequiredMetadata.Contains(a.Key) && !OptionalValueMetadata.Contains(a.Key)) return false;

        if (RequiredMetadata.Contains(a.Key) && a.Value.Length == 0) return false;

        return true;
    }

    private Task<FileStream> SaveCompletedFileWithExtensionNew(string filePath, string extension)
    {
        using var rawFS = new FileStream(filePath, FileMode.Open);
        var formatedFS = new FileStream($"{filePath}.{extension}", FileMode.OpenOrCreate);

        rawFS.CopyTo(formatedFS);
        formatedFS.Seek(0, SeekOrigin.Begin);

        return Task.FromResult(formatedFS);
    }

    public static bool MetadataValidator(Dictionary<string, string> metadata)
    {
        if (metadata is null || metadata.Count == 0) return false;

        var isMetadataPresent = RequiredMetadata.All(metadata.ContainsKey) && OptionalValueMetadata.All(metadata.ContainsKey);
        if (!isMetadataPresent)
        {
            return false;
        }

        var isMetadataValuesFilled = metadata.All(ValidateMetadataEntry);

        return isMetadataPresent && isMetadataValuesFilled;
    }

    public static string GetDirectory()
    {
        var tempDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine("/tmp", "sf-photos", "temp") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sf-photos", "temp");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }
        return tempDirectory;
    }
}
