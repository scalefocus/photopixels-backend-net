using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Security;
using SF.PhotoPixels.Domain.Utils;

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
            if (request.Name.IsValidEmail())
            {
                return new ValidationError("IllegalUserInput", "User input cannot be an email address");
            }

            if (!request.Email.IsValidEmail())
            {
                return new ValidationError("IllegalEmailInput", "Email input must be an email address");
            }

            var result = await _userManager.CreateAsync(MapRequestToUser(request), request.Password);

			return result.Succeeded ? 
				new Success() : 
				result.CreateValidationError();
		}

		private static Domain.Entities.User MapRequestToUser(AdminRegisterRequest request)
		{
			return new Domain.Entities.User
			{
				Name = request.Name,
				Email = request.Email,
				UserName = request.Name,
				Role = request.Role,
				EmailConfirmed = true,
			};
		}

    }
}