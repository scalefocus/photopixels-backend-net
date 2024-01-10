using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.GetUserInfo;

public class GetUserInfoRequest : IQuery<OneOf<GetUserInfoResponse, NotFound>>
{
    public static GetUserInfoRequest Instance => LazyInstance.Value;
    private static readonly Lazy<GetUserInfoRequest> LazyInstance = new(() => new GetUserInfoRequest());

    private GetUserInfoRequest()
    {
    }
}