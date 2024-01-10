using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Security;

namespace SF.PhotoPixels.Application.Commands.User.Register
{
    public class AdminRegisterHandler : IRequestHandler<AdminRegisterRequest, OneOf<Success, ValidationError>>
    {
        private readonly UserManager<Domain.Entities.User> _userManager;

        public AdminRegisterHandler(UserManager<Domain.Entities.User> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<OneOf<Success, ValidationError>> Handle(AdminRegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _userManager.CreateAsync(MapRequestToUser(request), request.Password);

            if (result.Succeeded)
            {
                return new Success();
            }

            return result.CreateValidationError();
        }

        private static Domain.Entities.User MapRequestToUser(AdminRegisterRequest request)
        {
            return new Domain.Entities.User
            {
                Name = request.Name,
                Email = request.Email,
                UserName = request.Email,
                Role = request.Role,
                EmailConfirmed = true,
            };
        }
    }
}