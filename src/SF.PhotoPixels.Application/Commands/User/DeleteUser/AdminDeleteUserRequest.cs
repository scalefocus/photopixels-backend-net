using Mediator;

namespace SF.PhotoPixels.Application.Commands.User.DeleteUser;

public class AdminDeleteUserRequest : IRequest<DeleteUserResponse>
{
    public required Guid Id { get; set; }
}
