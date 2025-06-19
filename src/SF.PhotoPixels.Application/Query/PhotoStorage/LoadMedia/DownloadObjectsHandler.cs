using System.IO.Compression;
using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class DownloadObjectsHandler : IRequestHandler<DownloadObjectsRequest, OneOf<byte[], NotFound>>
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

    public async ValueTask<OneOf<byte[], NotFound>> Handle(DownloadObjectsRequest request, CancellationToken cancellationToken)
    {
        var objects = await _session.Query<ObjectProperties>()
            .Where(obj => request.ObjectIds.Contains(obj.Id) && _executionContextAccessor.UserId == obj.UserId)
            .Select(x => new { x.Hash, x.Extension, x.Name })
            .ToListAsync(cancellationToken);

        if (objects.Count == 0)
            return new NotFound();

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

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }
}