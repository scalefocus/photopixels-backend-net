using Marten;
using Marten.Linq.SoftDeletes;
using Microsoft.Extensions.Logging;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Repositories;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class TrashHardDeleteService : ITrashHardDeleteService
{

    private readonly IDocumentSession _session;

    private readonly ILogger<TrashHardDeleteService> _logger;

    private readonly IApplicationConfigurationRepository _applicationConfigurationRepository;

    public TrashHardDeleteService(
        IDocumentSession session,
        ILogger<TrashHardDeleteService> logger,
        IApplicationConfigurationRepository applicationConfigurationRepository)
    {
        _session = session;
        _logger = logger;
        _applicationConfigurationRepository = applicationConfigurationRepository;
    }

    public async Task<IEnumerable<string>> EmptyTrashBin(Guid userid)
    {
        if (userid == Guid.Empty)
        {
            _logger.LogWarning("EmptyTrashBin called with invalid user id: {UserId}", userid);
            return new List<string>();
        }

        _logger.LogInformation("Emptying trash bin for {UserId}", userid);

        var itemsToRemoveExpr = _session.Query<ObjectProperties>()
                                                   .Where(x => x.IsDeleted() && x.UserId == userid);

        _logger.LogInformation("Emptying trash bin for {UserId} done", userid);
        return await itemsToRemoveExpr.Select(x => x.Id).ToListAsync();
    }

    public async Task EmptyTrashBin(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Emptying trash bin");
        var _applicationConfiguration = await _applicationConfigurationRepository.GetConfiguration();

        var _lastRun = _applicationConfiguration.GetValue<DateTimeOffset>("TrashHardDeleteConfiguration.LastRun");
        _lastRun = _lastRun == default ? DateTimeOffset.MinValue : _lastRun;

        var _deleteAtTimeOfDay = _applicationConfiguration.GetValue<TimeOnly>("TrashHardDeleteConfiguration.DeleteAtTimeOfDay");
        _deleteAtTimeOfDay = _deleteAtTimeOfDay == default ? new TimeOnly(0, 0, 0) : _deleteAtTimeOfDay;

        var timeOfDaySeconds = _deleteAtTimeOfDay.ToTimeSpan().TotalSeconds;
        var now = DateTime.UtcNow;
        if (now.TimeOfDay.TotalSeconds > timeOfDaySeconds
                && now.Date > _lastRun.Date.AddDays(1))
        {
            await EmptyTrashBin(Guid.Empty);
        }

        _logger.LogInformation("Emptying trash bin done");
    }

}
