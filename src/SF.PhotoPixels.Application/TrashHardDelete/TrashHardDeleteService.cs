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

    private TimeOnly _deleteAtTimeOfDay = new TimeOnly(0, 0, 0);

    private int _daysToDelayHardDelete = 30;

    private DateTimeOffset _lastRun = DateTimeOffset.MinValue;

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
        var _applicationConfiguration = await _applicationConfigurationRepository.GetConfiguration();
        var _daysToDelayHardDeleteConfigValue = _applicationConfiguration.GetValue<int>("TrashHardDeleteConfiguration.DaysToDelayHardDelete");
        _daysToDelayHardDelete = _daysToDelayHardDeleteConfigValue == default ? _daysToDelayHardDelete : _daysToDelayHardDeleteConfigValue;

        _logger.LogInformation($"Emptying trash bin for {userid}");

        var nowDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var treshholdDate = new DateTimeOffset(nowDate.Year, nowDate.Month, nowDate.Day, 0, 0, 0, TimeSpan.Zero).AddDays(-_daysToDelayHardDelete);
        var itemsToRemoveExpr = _session.Query<ObjectProperties>()
                                                   .Where(x => x.IsDeleted()
                                                               && x.DeletedAt < treshholdDate);
        if (userid != Guid.Empty)
        {
            itemsToRemoveExpr = itemsToRemoveExpr.Where(x => x.UserId == userid);
        }

        _logger.LogInformation($"Emptying trash bin for {userid} done");
        return await itemsToRemoveExpr.Select(x => x.Id).ToListAsync();

    }

    public async Task EmptyTrashBin(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Emptying trash bin");
        var _applicationConfiguration = await _applicationConfigurationRepository.GetConfiguration();
        _lastRun = _applicationConfiguration.GetValue<DateTimeOffset>("TrashHardDeleteConfiguration.LastRun");
        _deleteAtTimeOfDay = _applicationConfiguration.GetValue<TimeOnly>("TrashHardDeleteConfiguration.DeleteAtTimeOfDay");
        _deleteAtTimeOfDay = _deleteAtTimeOfDay == default ? new TimeOnly(0, 0, 0) : _deleteAtTimeOfDay;

        var timeOfDaySeconds = _deleteAtTimeOfDay.Hour * 3600 + _deleteAtTimeOfDay.Minute * 60 + _deleteAtTimeOfDay.Second;
        var now = DateTimeOffset.Now;
        if (now.TimeOfDay.TotalSeconds > timeOfDaySeconds
                && now.Date > _lastRun.Date.AddDays(1))
        {
            await EmptyTrashBin(Guid.Empty);
        }

        _logger.LogInformation("Emptying trash bin done");
    }

}
