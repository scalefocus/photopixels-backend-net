using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Utils;
using SF.PhotoPixels.Infrastructure.Storage;
using System.IO.Compression;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class DownloadObjectsHandler : IRequestHandler<DownloadObjectsRequest, OneOf<DownloadObjectsResult, NotFound>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly IObjectStorage _objectStorage;

    public DownloadObjectsHandler(IExecutionContextAccessor executionContextAccessor, IDocumentSession session, IObjectStorage objectStorage)
    {
        _executionContextAccessor = executionContextAccessor;
        _session = session;
        _objectStorage = objectStorage;
    }

    public async ValueTask<OneOf<DownloadObjectsResult, NotFound>> Handle(DownloadObjectsRequest request, CancellationToken cancellationToken)
    {
        var objects = await _session.Query<ObjectProperties>()
            .Where(obj => request.ObjectIds.Contains(obj.Id) && _executionContextAccessor.UserId == obj.UserId)
            .ToListAsync(cancellationToken);

        return objects.Count switch
        {
            0 => new NotFound(),
            1 => await HandleSingleObjectAsync(objects[0], cancellationToken),
            _ => await HandleMultipleObjectsAsync(objects, cancellationToken)
        };
    }

    private async ValueTask<DownloadObjectsResult> HandleSingleObjectAsync(ObjectProperties obj, CancellationToken cancellationToken)
    {
        var userFolders = _objectStorage.GetUserFolders(_executionContextAccessor.UserId);
        var filePath = Path.Combine(userFolders.ObjectFolder, $"{obj.Hash}.{obj.Extension}");
        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return new DownloadObjectsResult
        {
            FileBytes = fileBytes,
            FileName = obj.Name,
            ContentType = MimeTypes.GetMimeTypeFromExtension(obj.Extension)
        };
    }

    private async ValueTask<DownloadObjectsResult> HandleMultipleObjectsAsync(IReadOnlyList<ObjectProperties> objects, CancellationToken cancellationToken)
    {
        var userFolders = _objectStorage.GetUserFolders(_executionContextAccessor.UserId);
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var obj in objects)
            {
                var filePath = Path.Combine(userFolders.ObjectFolder, $"{obj.Hash}.{obj.Extension}");
                var entry = zipArchive.CreateEntry(obj.Name, CompressionLevel.Optimal);

                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                await using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        return new DownloadObjectsResult
        {
            FileBytes = memoryStream.ToArray(),
            FileName = "files.zip",
            ContentType = "application/zip"
        };
    }
}