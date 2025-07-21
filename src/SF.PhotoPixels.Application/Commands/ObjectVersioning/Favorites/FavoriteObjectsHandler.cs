using Marten;
using Mediator;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.Favorites;

public class FavoriteObjectsHandler : IRequestHandler<FavoriteObjectsRequest, FavoriteObjectsResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public FavoriteObjectsHandler(
        IDocumentSession session,
        IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<FavoriteObjectsResponse> Handle(FavoriteObjectsRequest request, CancellationToken cancellationToken)
    {
        var objects = await _session.Query<ObjectProperties>()
            .Where(obj => request.ObjectIds.Contains(obj.Id) && obj.UserId == _executionContextAccessor.UserId)
            .ToListAsync();

        foreach (var obj in objects)
        {
            obj.IsFavorite = request.FavoriteActionType switch
            {
                Domain.Enums.FavoriteActionType.RemoveFromFavorites => false,
                Domain.Enums.FavoriteActionType.AddToFavorites => true,
                _ => throw new ArgumentException("Invalid favorite action type", nameof(request.FavoriteActionType)),
            };
            _session.Update(obj);
        }
        await _session.SaveChangesAsync(cancellationToken);

        return new FavoriteObjectsResponse();
    }
}

