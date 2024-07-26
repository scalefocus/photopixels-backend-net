using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.GetUser
{
    public class GetUserRequest : IQuery<OneOf<GetUserResponse, None>>
    {
        public required string Id { get; set; }
    }
}