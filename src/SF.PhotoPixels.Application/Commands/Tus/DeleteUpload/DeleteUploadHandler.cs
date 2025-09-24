using Mediator;
using Microsoft.AspNetCore.Identity;
using SF.PhotoPixels.Application.Core;
using SolidTUS.Handlers;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadHandler : IRequestHandler<DeleteUploadRequest, DeleteUploadResponses>
{
    private readonly IUploadMetaHandler _uploadMetaHandler;
    private readonly IUploadStorageHandler _uploadStorageHandler;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public DeleteUploadHandler(IUploadMetaHandler uploadMetaHandler, IUploadStorageHandler uploadStorageHandler, IExecutionContextAccessor executionContextAccessor, UserManager<Domain.Entities.User> userManager)
    {
        _uploadMetaHandler = uploadMetaHandler;
        _uploadStorageHandler = uploadStorageHandler;
        _executionContextAccessor = executionContextAccessor;
        _userManager = userManager;
    }

    public async ValueTask<DeleteUploadResponses> Handle(DeleteUploadRequest request, CancellationToken cancellationToken)
    {
        var info = await _uploadMetaHandler.GetResourceAsync(request.FileId, cancellationToken);
        if (info?.Metadata == null || !info.Metadata.TryGetValue("userId", out var metaUserId))
        {
            return new ValidationError("Object not found", "The object has already been deleted or user not found in metadata");
        }

        var userId = _executionContextAccessor.UserId.ToString();
        if (!userId.Equals(metaUserId))
        {
            return new ValidationError("User not found", "Compared users are not the same");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new ValidationError("User not found", "User not found in context");
        }

        await _uploadMetaHandler.DeleteUploadFileInfoAsync(info, cancellationToken);
        await _uploadStorageHandler.DeleteFileAsync(info, cancellationToken);
        return new DeleteUploadResponse();
    }
}
