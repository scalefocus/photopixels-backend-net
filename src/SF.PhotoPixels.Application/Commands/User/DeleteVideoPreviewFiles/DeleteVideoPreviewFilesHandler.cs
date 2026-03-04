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

        var previewVideosFolders = _objectStorage.GetUserConvertedVideosFolder(_executionContextAccessor.UserId);
        var filesToConvert = new DirectoryInfo(previewVideosFolders)
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Select(x => x.FullName)
            .ToList();

        if (filesToConvert.Count == 0)
        {
            new Success();
        }

        foreach (var file in filesToConvert)
        {
            if (_objectStorage.DeletePreview(user.Id, file, out long fileSize))
                user.DecreaseUsedQuota(fileSize);
        }

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}