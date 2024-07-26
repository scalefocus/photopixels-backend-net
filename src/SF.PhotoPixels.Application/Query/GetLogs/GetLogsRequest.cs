using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Query.GetLogs;

public class GetLogsRequest : IQuery<OneOf<Stream, None>>
{
    public static GetLogsRequest Instance => LazyInstance.Value;
    private static readonly Lazy<GetLogsRequest> LazyInstance = new(() => new GetLogsRequest());

    private GetLogsRequest()
    {
    }
}