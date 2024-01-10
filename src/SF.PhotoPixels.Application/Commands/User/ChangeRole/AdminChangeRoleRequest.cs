using Mediator;
using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.Application.Commands.User.ChangeRole;

public class AdminChangeRoleRequest : IRequest<AdminChangeRoleResponse>
{
    public required Guid Id { get; set; }

    public required Role Role { get; set; }
}
