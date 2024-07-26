using Mediator;
using SF.PhotoPixels.Application.Core;
using SolidTUS.Extensions;
using SolidTUS.Contexts;
using SF.PhotoPixels.Infrastructure.Services.TusService;
using SF.PhotoPixels.Domain.Entities;
using Marten;
using System.Text;

namespace SF.PhotoPixels.Application.Commands.Tus.CreateUpload;

public class CreateUploadHandler : IRequestHandler<CreateUploadRequest, CreateUploadResponses>
{

    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly ITusService _tusService;
    private readonly IDocumentSession _session;

    public CreateUploadHandler(IExecutionContextAccessor executionContextAccessor, ITusService tusService, IDocumentSession session)
    {
        _executionContextAccessor = executionContextAccessor;
        _tusService = tusService;
        _session = session;
    }

    public async ValueTask<CreateUploadResponses> Handle(CreateUploadRequest request, CancellationToken cancellationToken)
    {

        var metadata = _executionContextAccessor.HttpContext!.TusInfo().Metadata;
        var userId = _executionContextAccessor.UserId;

        var user = _session.Load<Domain.Entities.User>(userId);
        if (user is null){

            return new ValidationError("User not found", "User not found");
        }

        var imageFingerprint = new StringBuilder(metadata["fileHash"]).Replace('+', '-').Replace('/', '_').Replace("=", "").ToString();
        var objectId = new ObjectId(userId, imageFingerprint);

        if (await _session.Query<ObjectProperties>().AnyAsync(x => x.Id == objectId, cancellationToken))
        {
            return new ValidationError("Object hash does not match", "Object hash does not match");
        }

        var fileSize = long.Parse(_executionContextAccessor.HttpContext!.Request.Headers["Upload-Length"]!);
        if (!user.IncreaseUsedQuota(fileSize))
        {
            return new ValidationError("User quota is reached", "User quota is reached, not enough capacity for new upload");
        }

        var myFileId = Guid.NewGuid().ToString();
        TusCreationContext ctx = _executionContextAccessor.HttpContext.TusCreation(myFileId).OnCreateWithUploadFinished(_tusService.HandleNewCompletion).Build("fileId");

        await ctx.StartCreationAsync(_executionContextAccessor.HttpContext);

        return new CreateUploadResponse();
    }
}
