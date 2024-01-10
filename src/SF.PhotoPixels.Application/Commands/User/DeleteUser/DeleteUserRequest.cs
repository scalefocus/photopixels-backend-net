using Mediator;

namespace SF.PhotoPixels.Application.Commands.User.DeleteUser;

public class DeleteUserRequest : IRequest<DeleteUserResponse>
{
    public required string Password { get; set; }
}
