using Marten;
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
        if (request.LastId == null)
        {
            sqlQuery = $@"
                SELECT  data
                FROM photos.mt_doc_objectproperties
                WHERE (data->>'DateCreated')::timestamptz <= :timeNow
                        AND (data->>'UserId')::uuid = :userId
                        AND (data->>'TrashDate')::timestamptz is not null
                ORDER BY data->>'DateCreated' desc ,id desc
                FETCH FIRST :pageSize ROWS ONLY";
        }
        else
        {
            sqlQuery = $@"
                SELECT  data
                FROM photos.mt_doc_objectproperties
                WHERE ((data->>'DateCreated')::timestamptz = (SELECT (data->>'DateCreated')::timestamptz
                                                              FROM photos.mt_doc_objectproperties
                                                              WHERE id = :lastId)

                      AND id <= :lastId) OR
                      ((data->>'DateCreated')::timestamptz < (SELECT (data->>'DateCreated')::timestamptz
                                                              FROM photos.mt_doc_objectproperties
                                                              WHERE id = :lastId))
                      AND (data->>'UserId')::uuid = :userId
                      AND (data->>'TrashDate')::timestamptz is not null
                ORDER BY data->>'DateCreated' desc ,id desc
                FETCH FIRST :pageSize ROWS ONLY";
        }

        var result = await _session.QueryAsync<ObjectProperties>(sqlQuery, new { pageSize = request.PageSize + 1, timeNow = utcNow, lastId = request.LastId, userId = _executionContextAccessor.UserId });
        var objectProperties = result.ToList();

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
                DateTrashed = obj.TrashDate
            };

            properties.Add(thumbnailProperty);
        }
       
        var lastId = result.Count < request.PageSize ? "" : objectProperties[^1].Id;
        return new GetObjectsTrashedResponse() { Properties = properties, LastId = lastId };
    }
}