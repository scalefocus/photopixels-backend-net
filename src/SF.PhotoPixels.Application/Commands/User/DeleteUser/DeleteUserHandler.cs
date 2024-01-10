using Marten;
using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.User.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IDocumentSession _session;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public DeleteUserHandler(IObjectStorage objectStorage, IDocumentSession session, IExecutionContextAccessor executionContextAccessor, UserManager<Domain.Entities.User> userManager)
    {
        _objectStorage = objectStorage;
        _session = session;
        _executionContextAccessor = executionContextAccessor;
        _userManager = userManager;
    }

    public async ValueTask<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _session.Query<Domain.Entities.User>()
            .SingleOrDefaultAsync(x => x.Id == _executionContextAccessor.UserId, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        if (!await CanDelete(request.Password, user))
        {
            return new Forbidden();
        }

        _objectStorage.DeleteUserFolders(_executionContextAccessor.UserId);
        _session.HardDeleteWhere<ObjectProperties>(x => x.UserId == _executionContextAccessor.UserId);
        _session.Delete(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    private async Task<bool> CanDelete(string password, Domain.Entities.User user)
    {
        if (user.Role == Role.Admin)
        {
            var anotherAdmin = await _session.Query<Domain.Entities.User>().Where(u => u.Role == Role.Admin && u.Id != user.Id).SingleOrDefaultAsync();
            if (anotherAdmin == null)
            {
                return false;
            }
            return true;
        }

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);
        if (isPasswordCorrect)
        {
            return true;
        }

        return false;
    }
}