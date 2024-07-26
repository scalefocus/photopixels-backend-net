using Mediator;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;

public class GetObjectDataRequest : IQuery<QueryResponse<ObjectDataResponse>>
{
    public required string ObjectId { get; set; }
}
