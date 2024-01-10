using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Security;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Enums;
using SF.PhotoPixels.Domain.Repositories;

namespace SF.PhotoPixels.Application.Commands.User.Register
{
    public class UserRegisterHandler : IRequestHandler<UserRegisterRequest, OneOf<Success, ValidationError>>
    {
        private readonly IApplicationConfigurationRepository _configurationRepository;
        private readonly UserManager<Domain.Entities.User> _userManager;

        public UserRegisterHandler(UserManager<Domain.Entities.User> userManager, IApplicationConfigurationRepository configurationRepository)
        {
            _userManager = userManager;
            _configurationRepository = configurationRepository;
        }

        public async ValueTask<OneOf<Success, ValidationError>> Handle(UserRegisterRequest request, CancellationToken cancellationToken)
        {
            var appConfig = await _configurationRepository.GetConfiguration();
            if (!appConfig.GetValue<bool>(ConfigurationConstants.Registration))
            {
                return new ValidationError("RegistrationIsDisabled", "Cannot register new user until registration is enabled by admin!");
            }

            var result = await _userManager.CreateAsync(MapRequestToUser(request), request.Password);

            if (result.Succeeded)
            {
                return new Success();
            }

            return result.CreateValidationError();
        }

        private static Domain.Entities.User MapRequestToUser(UserRegisterRequest request)
        {
            return new Domain.Entities.User
            {
                Name = request.Name,
                Email = request.Email,
                UserName = request.Email,
                Role = Role.Contributor,
                EmailConfirmed = true,
            };
        }
    }
}