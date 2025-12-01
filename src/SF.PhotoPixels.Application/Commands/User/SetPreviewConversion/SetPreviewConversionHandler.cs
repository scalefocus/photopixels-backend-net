using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using OneOf;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.User.SetPreviewConversion;

public class SetPreviewConversionHandler : IRequestHandler<SetPreviewConversionRequest, OneOf<Success, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectRepository _objectRepository;

    public SetPreviewConversionHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;
        _objectRepository = objectRepository;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(SetPreviewConversionRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_executionContextAccessor.UserId.ToString());

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var evt = new PreviewConversionSet
        {
            UserId = _executionContextAccessor.UserId,
            PreviewConversion = request.PreviewConversion,
        };
        // todo: register the event in any projection in order to store it
        await _objectRepository.AddEvent(evt.UserId, evt, cancellationToken);
        return new Success();
    }
}
