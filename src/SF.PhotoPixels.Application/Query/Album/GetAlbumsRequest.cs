using Mediator;
using OneOf;

namespace SF.PhotoPixels.Application.Query.Album;

public class GetAlbumsRequest : IQuery<OneOf<GetAlbumsResponse, ValidationError>>
{
    public static GetAlbumsRequest Instance => LazyInstance.Value;
    private static readonly Lazy<GetAlbumsRequest> LazyInstance = new(() => new GetAlbumsRequest());

    private GetAlbumsRequest()
    {
    }
}