using Mediator;
using SF.PhotoPixels.Application.Core;
using SolidTUS.Handlers;

namespace SF.PhotoPixels.Application.Query.GetUserUploads;

public class GetUserUploadsHandler : IRequestHandler<GetUserUploadsRequest, GetUserUploadsResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IUploadMetaHandler _uploadMetaHandler;

    public GetUserUploadsHandler(
        IExecutionContextAccessor executionContextAccessor,
        IUploadMetaHandler uploadMetaHandler
        )
    {
        _executionContextAccessor = executionContextAccessor;
        _uploadMetaHandler = uploadMetaHandler;
    }

    public async ValueTask<GetUserUploadsResponse> Handle(GetUserUploadsRequest request, CancellationToken cancellationToken)
    {
        var userId = _executionContextAccessor.UserId;

        var fileInfos = _uploadMetaHandler.GetAllResourcesAsync();

        var userFiles = new List<UserUploadData>();
        await foreach (var item in fileInfos)
        {
            var hasId = Guid.TryParse(item.Metadata["userId"], out var id);
            if (hasId && id.Equals(userId))
            {
                var uploadInfo = new UserUploadData()
                {
                    FileId = item.FileId,
                    Expiration = item.ExpirationDate,
                    ByteOffset = item.ByteOffset,
                    FileSize = item.FileSize,
                    Metadata = item.Metadata,
                    Creation = item.CreatedDate
                };
                userFiles.Add(uploadInfo);
            }
        }

        return new GetUserUploadsResponse()
        {
            UserUploads = userFiles
        };
    }
}
