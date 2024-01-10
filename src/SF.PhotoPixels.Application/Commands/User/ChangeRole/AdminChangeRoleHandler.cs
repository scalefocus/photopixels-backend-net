using Marten;
using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Commands.User.ChangeRole;

public class AdminChangeRoleHandler : IRequestHandler<AdminChangeRoleRequest, AdminChangeRoleResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public AdminChangeRoleHandler(IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<AdminChangeRoleResponse> Handle(AdminChangeRoleRequest request, CancellationToken cancellationToken)
    {
        //the admin user should not be able to change his/her own role
        if (_executionContextAccessor.UserId == request.Id)
        {
            return new Forbidden();
        }

        var user = await _session.Query<Domain.Entities.User>()
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        user.Role = request.Role;

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}