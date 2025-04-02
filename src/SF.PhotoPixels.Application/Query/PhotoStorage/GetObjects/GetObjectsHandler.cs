using Marten;
using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using Weasel.Postgresql.Tables.Partitioning;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class GetObjectsHandler : IQueryHandler<GetObjectsRequest, OneOf<GetObjectsResponse, None>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public GetObjectsHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<GetObjectsResponse, None>> Handle(GetObjectsRequest request, CancellationToken cancellationToken)
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
                ORDER BY data->>'DateCreated' desc ,id desc
                FETCH FIRST :pageSize ROWS ONLY";
        }

        var result = await _session.QueryAsync<ObjectProperties>(sqlQuery, new { pageSize = request.PageSize + 1, timeNow = utcNow, lastId = request.LastId, userId = _executionContextAccessor.UserId });
        var objectProperties = result.ToList();

        var properties = new List<PropertiesResponse>();

        if (!objectProperties.Any())
        {
            return new None();
        }

        foreach (var obj in objectProperties.Take(request.PageSize))
        {
            var thumbnailProperty = new PropertiesResponse
            {
                Id = obj.Id,
                DateCreated = obj.DateCreated,
            };

            properties.Add(thumbnailProperty);
        }


        var lastId = result.Count < request.PageSize ? "" : objectProperties[^1].Id;
        return new GetObjectsResponse() { Properties = properties, LastId = lastId };
    }
}