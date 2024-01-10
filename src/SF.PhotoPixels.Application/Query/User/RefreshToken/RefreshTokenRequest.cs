using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.RefreshToken;

public class RefreshTokenRequest : IQuery<OneOf<Success, Unauthorized>>
{
    public required string RefreshToken { get; init; }
}