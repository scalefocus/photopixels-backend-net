using Mediator;
using SolidTUS.Handlers;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadHandler : IRequestHandler<DeleteUploadRequest, DeleteUploadResponses>
{

    private readonly IUploadMetaHandler _uploadMetaHandler;
    private readonly IUploadStorageHandler _uploadStorageHandler;

    public DeleteUploadHandler(IUploadMetaHandler uploadMetaHandler, IUploadStorageHandler uploadStorageHandler)
    {
        _uploadMetaHandler = uploadMetaHandler;
        _uploadStorageHandler = uploadStorageHandler;
    }

    public async ValueTask<DeleteUploadResponses> Handle(DeleteUploadRequest request, CancellationToken cancellationToken)
    {
        var info = await _uploadMetaHandler.GetResourceAsync(request.FileId, cancellationToken);

        if (info is null)
        {
            return new ValidationError("Object not found", "The object has already been deleted");            
        }

        await _uploadMetaHandler.DeleteUploadFileInfoAsync(info, cancellationToken);        
        await _uploadStorageHandler.DeleteFileAsync(info, cancellationToken);
        return new DeleteUploadResponse();
    }
}
