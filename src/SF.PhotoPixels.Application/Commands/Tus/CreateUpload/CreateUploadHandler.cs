using Mediator;
using SF.PhotoPixels.Application.Core;
using SolidTUS.Extensions;
using SolidTUS.Contexts;
using Microsoft.AspNetCore.Identity;
using System.Text;
using SF.PhotoPixels.Domain.Entities;
using Marten;

namespace SF.PhotoPixels.Application.Commands.Tus.CreateUpload;

public class CreateUploadHandler : IRequestHandler<CreateUploadRequest, CreateUploadResponses>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public CreateUploadHandler(IExecutionContextAccessor executionContextAccessor, UserManager<Domain.Entities.User> userManager, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _userManager = userManager;
        _session = session;
    }

    public async ValueTask<CreateUploadResponses> Handle(CreateUploadRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_executionContextAccessor.UserId.ToString());
        if (user is null)
        {
            return new ValidationError("User not found", "User not found");
        }

        var metadata = _executionContextAccessor.HttpContext!.TusInfo().Metadata;
        var imageFingerprint = new StringBuilder(metadata["fileHash"]).Replace('+', '-').Replace('/', '_').Replace("=", "").ToString();
        var objectId = new ObjectId(_executionContextAccessor.UserId, imageFingerprint);
        if (await _session.Query<ObjectProperties>().AnyAsync(x => x.Id == objectId, cancellationToken))
        {
            return new ValidationError("Duplicate photo", "The photo has already been uploaded");
        }

        var fileSize = long.Parse(_executionContextAccessor.HttpContext!.Request.Headers["Upload-Length"]!);
        // Although quota is increased, this is not saved and only used to validate that the photo can be saved. The actual increase of quota is done upon complete upload of the photo in TusService
        if (!user.IncreaseUsedQuota(fileSize))
        {
            return new ValidationError("User quota is reached", "User quota is reached, not enough capacity for new upload");
        }

        var myFileId = Guid.NewGuid().ToString();
        TusCreationContext ctx = _executionContextAccessor.HttpContext.TusCreation(myFileId).Build("fileId");

        await ctx.StartCreationAsync(_executionContextAccessor.HttpContext);

        return new CreateUploadResponse();
    }
}
