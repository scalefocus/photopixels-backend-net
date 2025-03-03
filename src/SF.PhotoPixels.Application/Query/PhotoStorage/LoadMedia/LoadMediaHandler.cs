using Marten;
using Mediator;
using Microsoft.Extensions.Options;
using OneOf.Types;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadPhoto;

public class LoadMediaHandler : IQueryHandler<LoadMediaRequest, QueryResponse<LoadMediaResponse>>, IDisposable, IAsyncDisposable
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly SystemConfig _systemConfig;

    public LoadMediaHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, IOptionsMonitor<SystemConfig> systemConfigOptions)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _systemConfig = systemConfigOptions.CurrentValue;
    }

    public async ValueTask DisposeAsync()
    {
        await _session.DisposeAsync();
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaRequest request, CancellationToken cancellationToken)
    {
        var metadata = await _session.Query<ObjectProperties>()
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == _executionContextAccessor.UserId, cancellationToken);

        if (metadata == null)
        {
            return new NotFound();
        }

        var mediaHandler = MediaCreationFactory.CreateMediaHandler(metadata, request, _executionContextAccessor, _objectStorage, _session, _systemConfig);
        var response = await mediaHandler.Handle(metadata, cancellationToken);
        return response;
    }
}