using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.User.DeleteUser;

public class AdminDeleteUserHandler : IRequestHandler<AdminDeleteUserRequest, DeleteUserResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;

    public AdminDeleteUserHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<DeleteUserResponse> Handle(AdminDeleteUserRequest request, CancellationToken cancellationToken)
    {
        if (!await CanDelete(request.Id))
        {
            return new Forbidden();
        }

        var user = await _session.Query<Domain.Entities.User>()
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        _objectStorage.DeleteUserFolders(request.Id);
        _session.HardDeleteWhere<ObjectProperties>(x => x.UserId == request.Id);
        _session.Delete(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    private async Task<bool> CanDelete(Guid userId)
    {
        if (_executionContextAccessor.UserId == userId)
        {
            var anotherAdmin = await _session.Query<Domain.Entities.User>().Where(u=>u.Role == Role.Admin && u.Id != userId).SingleOrDefaultAsync();
            if (anotherAdmin == null)
            {
                return false;
            }
        }
        return true;
    }
}