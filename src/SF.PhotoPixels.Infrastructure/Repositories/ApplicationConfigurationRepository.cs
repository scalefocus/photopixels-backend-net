using Marten;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Repositories;

namespace SF.PhotoPixels.Infrastructure.Repositories;

public class ApplicationConfigurationRepository : IApplicationConfigurationRepository
{
    private readonly IDocumentSession _documentSession;

    public ApplicationConfigurationRepository(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<ApplicationConfiguration> GetConfiguration()
    {
        return await _documentSession.Query<ApplicationConfiguration>().FirstOrDefaultAsync() ?? new ApplicationConfiguration();
    }

    public Task SaveConfiguration(ApplicationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _documentSession.Store(configuration);

        return _documentSession.SaveChangesAsync(cancellationToken);
    }
}