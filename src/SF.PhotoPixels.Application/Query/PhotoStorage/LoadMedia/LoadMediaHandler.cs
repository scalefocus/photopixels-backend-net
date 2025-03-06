using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Models;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadPhoto;

public class LoadMediaHandler : IQueryHandler<LoadMediaRequest, QueryResponse<LoadMediaResponse>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;
    private readonly IMediaCreationFactory _mediaCreationFactory;

    public LoadMediaHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IMediaCreationFactory mediaCreationFactory)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _mediaCreationFactory = mediaCreationFactory;
    }


    public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaRequest request, CancellationToken cancellationToken)
    {
        var metadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == _executionContextAccessor.UserId, cancellationToken);

        if (metadata == null)
        {
            return new NotFound();
        }

        var mediaHandler = _mediaCreationFactory.CreateMediaHandler(metadata.Extension);
        var response = await mediaHandler.Handle(new LoadMediaCreationModel(metadata.GetFileName(), request.Format, metadata.MimeType), cancellationToken);
        return response;
    }
}