using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectData;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectDataList;

public class GetObjectDataListRequest : IRequest<OneOf<IList<ObjectDataResponse>,NotFound, ValidationError>>
{
    public required List<string> ObjectIds { get; set; }
}