using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class VideoPrevewDeleteBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private int _period = 60 * 60 * 24; // 24 hours

    public VideoPrevewDeleteBackgroundService(IServiceScopeFactory scopeFactory)
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
                var _objectStorage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();
                var _session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();

                var userIds = await _session.Query<User>()
                    .Where(x => !x.Settings.AllowVideoConversion)
                    .Select(x => x.Id)
                    .ToListAsync();

                if (userIds.Count == 0)
                {
                    continue;
                }

                foreach (var userId in userIds)
                {
                    var user = await _session.LoadAsync<User>(userId, cancellationToken);
                    if (user == null)
                    {
                        continue;
                    }

                    var previewVideosFolders = _objectStorage.GetUserConvertedVideosFolder(userId);
                    var filesToConvert = new DirectoryInfo(previewVideosFolders)
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Select(x => x.FullName)
                        .ToList();

                    if (filesToConvert.Count == 0)
                    {
                        continue;
                    }

                    foreach (var file in filesToConvert)
                    {
                        if (_objectStorage.DeletePreview(userId, file, out long fileSize))
                            user.DecreaseUsedQuota(fileSize);
                    }

                    _session.Update(user);
                    await _session.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}