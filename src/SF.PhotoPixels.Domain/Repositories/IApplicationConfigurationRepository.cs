using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Domain.Repositories;

public interface IApplicationConfigurationRepository
{
    public Task<ApplicationConfiguration> GetConfiguration();

    public Task SaveConfiguration(ApplicationConfiguration configuration, CancellationToken cancellationToken = default);
}