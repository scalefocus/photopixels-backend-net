using Mediator;

namespace SF.PhotoPixels.Application.Query.GetStatus;

public class GetStatusRequest : IQuery<QueryResponse<GetStatusResponse>>
{
    public static GetStatusRequest Instance => LazyInstance.Value;
    private static readonly Lazy<GetStatusRequest> LazyInstance = new(() => new GetStatusRequest());

    private GetStatusRequest()
    {
    }
}