using Marten;
using Marten.Linq.SoftDeletes;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectsTrashed;

public class GetObjectsTrashedHandler : IQueryHandler<GetObjectsTrashedRequest, OneOf<GetObjectsTrashedResponse, None>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public GetObjectsTrashedHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<GetObjectsTrashedResponse, None>> Handle(GetObjectsTrashedRequest request, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        string? sqlQuery;
        
        var result = _session.Query<ObjectProperties>()
            .Where(x => x.UserId == _executionContextAccessor.UserId 
                        && x.IsDeleted()
                        && x.DateCreated <= utcNow)  
            .OrderByDescending(x => x.DateCreated)
            .ThenByDescending(x => x.Id)
            .Take(request.PageSize + 1);

        if (request.LastId != null)
        {
            DateTimeOffset? lastDateCreated = !string.IsNullOrEmpty(request.LastId) 
                                    ? await _session.Query<ObjectProperties>().Where(x => x.Id == request.LastId).Select(x => x.DateCreated).FirstOrDefaultAsync()
                                    : await _session.Query<ObjectProperties>().Where(x=> x.UserId == _executionContextAccessor.UserId).OrderByDescending(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefaultAsync();

            result = result.Where(x => x.DateCreated <= lastDateCreated);
        }    
        
        var objectProperties = await result.ToListAsync();

        var properties = new List<PropertiesTrashedResponse>();

        if (!objectProperties.Any())
        {
            return new None();
        }

        foreach (var obj in objectProperties.Take(request.PageSize))
        {
            var thumbnailProperty = new PropertiesTrashedResponse
            {
                Id = obj.Id,
                DateCreated = obj.DateCreated,
                DateTrashed = obj.DeletedAt
            };

            properties.Add(thumbnailProperty);
        }
       
        var lastId = result.Count() < request.PageSize ? "" : objectProperties[^1].Id;
        return new GetObjectsTrashedResponse() { Properties = properties, LastId = lastId };
    }
}