using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.Albums;

public class GetAlbumsRequest : IQuery<OneOf<GetAlbumsResponse, ValidationError>>
{
    public static GetAlbumsRequest Instance => LazyInstance.Value;
    private static readonly Lazy<GetAlbumsRequest> LazyInstance = new(() => new GetAlbumsRequest());

    private GetAlbumsRequest()
    {
    }
}