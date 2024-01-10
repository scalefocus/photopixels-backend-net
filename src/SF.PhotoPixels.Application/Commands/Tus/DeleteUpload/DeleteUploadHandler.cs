using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SolidTUS.Handlers;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadHandler : IRequestHandler<DeleteUploadRequest, OneOf<NotFound, Success>>
{

    private readonly IUploadMetaHandler _uploadMetaHandler;
    private readonly IUploadStorageHandler _uploadStorageHandler;
    private readonly IDocumentSession _session;

    public DeleteUploadHandler(IUploadMetaHandler uploadMetaHandler, IUploadStorageHandler uploadStorageHandler, IDocumentSession session)
    {
        _uploadMetaHandler = uploadMetaHandler;
        _uploadStorageHandler = uploadStorageHandler;
        _session = session;
    }

    public async ValueTask<OneOf<NotFound, Success>> Handle(DeleteUploadRequest request, CancellationToken cancellationToken)
    {
        var info = await _uploadMetaHandler.GetResourceAsync(request.FileId, cancellationToken);

        if (info is null)
        {
            return new NotFound();
        }

        var userId = Guid.Parse(info.Metadata["userId"]);

        var user = _session.Load<Domain.Entities.User>(userId);
        if (user is null)
        {
            return new NotFound();
        }

        var fileSize = long.Parse(info.Metadata["fileSize"]!);
        user.DecreaseUsedQuota(fileSize);

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        await _uploadMetaHandler.DeleteUploadFileInfoAsync(info, cancellationToken);
        await _uploadStorageHandler.DeleteFileAsync(info, cancellationToken);
        return new Success();
    }
}
