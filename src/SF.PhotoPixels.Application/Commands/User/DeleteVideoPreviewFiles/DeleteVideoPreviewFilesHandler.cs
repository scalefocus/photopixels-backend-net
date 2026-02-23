using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.User.DeleteVideoPreviewFiles;

public class DeleteVideoPreviewFilesHandler : IRequestHandler<DeleteVideoPreviewFilesRequest, DeleteVideoPreviewFilesResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;

    public DeleteVideoPreviewFilesHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<DeleteVideoPreviewFilesResponse> Handle(DeleteVideoPreviewFilesRequest request, CancellationToken cancellationToken)
    {
        var user = await _session.Query<Domain.Entities.User>()
            .SingleOrDefaultAsync(x => x.Id == _executionContextAccessor.UserId, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        _objectStorage.DeleteUserConvertedVideos(_executionContextAccessor.UserId);
        return new Success();
    }
}