using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class TrashHardDeleteBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private int _period = 60 * 60 * 1; // 1 hour

    public TrashHardDeleteBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_period));
        while (
                !cancellationToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(cancellationToken))
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();
                var _logger = scope.ServiceProvider.GetRequiredService<ILogger<TrashHardDeleteBackgroundService>>();

                _logger.LogInformation(DateTimeOffset.Now + " - Running Trash Hard Delete");
                await mediatr.Send(new EmptyTrashBinRequest { UserId = Guid.Empty }, cancellationToken);
            }
        }
    }
}