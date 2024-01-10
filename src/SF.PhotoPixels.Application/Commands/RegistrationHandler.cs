using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Repositories;

namespace SF.PhotoPixels.Application.Commands
{
    public class RegistrationHandler : IRequestHandler<RegistrationRequest, OneOf<Success>>
    {
        private readonly IApplicationConfigurationRepository _configurationRepository;

        public RegistrationHandler(IApplicationConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async ValueTask<OneOf<Success>> Handle(RegistrationRequest request, CancellationToken cancellationToken)
        {
            var appConfig = await _configurationRepository.GetConfiguration();

            appConfig.SetValue(ConfigurationConstants.Registration, request.Value);

            await _configurationRepository.SaveConfiguration(appConfig, cancellationToken);

            return new Success();
        }
    }
}