using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using OneOf;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.User.AllowVideoConversion;

public class AllowVideoConversionHandler : IRequestHandler<AllowVideoConversionRequest, OneOf<Success, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;

    public AllowVideoConversionHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(AllowVideoConversionRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_executionContextAccessor.UserId.ToString());

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        user.Settings.AllowVideoConversion = request.AllowVideoConversion;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return new ValidationError("UserUpdateFailed", errorMessage);
        }

        return new Success();
    }
}
