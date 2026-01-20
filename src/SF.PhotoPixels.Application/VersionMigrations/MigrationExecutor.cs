using System.Collections.Concurrent;
using System.Reflection;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.PhotoPixels.Application.VersionMigrations.Versions;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.VersionMigrations;

public class MigrationExecutor
{
    private readonly ILogger<MigrationExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<Guid, Task<User>> _userCache = new();

    public MigrationExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MigrationExecutor>();
    }

    public async Task ExecuteMigrations(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var documentSession = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var appConfig = await documentSession.Query<ApplicationConfiguration>().FirstOrDefaultAsync(cancellationToken) ?? new ApplicationConfiguration();

        _logger.LogInformation("Executing migrations");

        var groupedMigrations = new Dictionary<int, MigrationVersionBase>();
        var migrationTypes = GetMigrations();

        foreach (var migrationType in migrationTypes)
        {
            var attribute = migrationType.GetCustomAttribute<MigrationVersionAttribute>();

            if (attribute is null)
            {
                continue;
            }

            var migration = Activator.CreateInstance(migrationType);

            groupedMigrations.Add(attribute.Version, (MigrationVersionBase)migration!);
        }

        var isFirstTime = appConfig.GetValue<bool?>(ConfigurationConstants.IsFirstTimeSetup) ?? true;

        if (isFirstTime)
        {
            var latestMigration = groupedMigrations.Max(x => x.Key);

            appConfig.SetValue(ConfigurationConstants.LatestMigration, latestMigration);

            documentSession.Store(appConfig);
            await documentSession.SaveChangesAsync(cancellationToken);

            return;
        }

        var latestMigrationVersion = appConfig.GetValue<int?>(ConfigurationConstants.LatestMigration) ?? 0;

        foreach (var (migrationNum, migration) in groupedMigrations.OrderBy(x => x.Key).Where(x => x.Key > latestMigrationVersion))
        {
            _logger.LogInformation("Executing migration {Migration}", migration.GetType().Name);

            var internalBeforeExecuteScope = scope.ServiceProvider.CreateAsyncScope();

            var beforeContext = new BeforeExecuteContext
            {
                Session = documentSession,
                Services = internalBeforeExecuteScope.ServiceProvider,
                Logger = _loggerFactory.CreateLogger<BeforeExecuteContext>(),
            };

            await migration.BeforeExecute(beforeContext);
            await documentSession.SaveChangesAsync(cancellationToken);

            await internalBeforeExecuteScope.DisposeAsync();

            if (beforeContext.IsStopRequested)
            {
                appConfig.SetValue(ConfigurationConstants.LatestMigration, migrationNum);
                documentSession.Store(appConfig);
                await documentSession.SaveChangesAsync(cancellationToken);

                continue;
            }

            var propertiesQuery = documentSession.Query<ObjectProperties>();
            var objectPropertisEnumerable = propertiesQuery.ToAsyncEnumerable(cancellationToken);
            var totalObjectsCount = propertiesQuery.Count();

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var processedObjectsCounter = new AtomicCounter();

            try
            {
                VersionMigrations.Log.TotalObjectsToProcess(_logger, totalObjectsCount);
                var objectStorage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();

                await Parallel.ForEachAsync(objectPropertisEnumerable, cts.Token, async (properties, token) =>
                {
                    await using var subSession = documentSession.DocumentStore.LightweightSession();
                    var user = await _userCache.GetOrAdd(properties.UserId, static async (key, args) => (await args.session.LoadAsync<User>(key, args.token))!, (session: subSession, token));

                    VersionMigrations.Log.ObjectMigrationExecutionStarting(_logger, properties.Name, user.Email);

                    var executeContext = new ExecuteContext(() => LoadImageAsync(objectStorage, properties))
                    {
                        ObjectId = properties.Id,
                        ObjectName = properties.Name,
                        MediaName = properties.Name,
                        User = user,
                        SaveImage = storageItem => objectStorage.StoreObjectAsync(user.Id, storageItem, properties.GetFileName(), token),
                        SaveThumbnail = storageItem => objectStorage.StoreThumbnailAsync(user.Id, storageItem, properties.GetThumbnailName(), token),
                        Logger = _loggerFactory.CreateLogger<ExecuteContext>(),
                    };

                    await migration.Execute(executeContext, beforeContext.FailOnError);

                    VersionMigrations.Log.ObjectMigrationExecutionFinished(_logger, properties.Name, user.Email);

                    VersionMigrations.Log.TotalObjectsProcessed(_logger, processedObjectsCounter.Next(), totalObjectsCount);

                    if (executeContext.IsStopRequested)
                    {
                        _logger.LogInformation("Stop requested. Cancelling execution.");
                        cts.Cancel();
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // ignored
            }

            foreach (var (_, value) in _userCache)
            {
                var user = await value;

                documentSession.Store(user);
            }

            await documentSession.SaveChangesAsync(cancellationToken);

            var internalAfterExecuteScope = scope.ServiceProvider.CreateAsyncScope();

            var afterContext = new AfterExecuteContext
            {
                Session = documentSession,
                Services = internalAfterExecuteScope.ServiceProvider,
                Logger = _loggerFactory.CreateLogger<AfterExecuteContext>(),
            };

            await migration.AfterExecute(afterContext);

            appConfig.SetValue(ConfigurationConstants.LatestMigration, migrationNum);
            documentSession.Store(appConfig);
            await documentSession.SaveChangesAsync(cancellationToken);

            await internalAfterExecuteScope.DisposeAsync();

            _logger.LogInformation("Executed migration {Migration}", migration.GetType().Name);
        }

        await documentSession.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("All migrations completed");
    }

    private static async Task<FormattedImage> LoadImageAsync(IObjectStorage objectStorage, ObjectProperties properties)
    {
        var rawImage = await objectStorage.LoadObjectAsync(properties.UserId, properties.GetFileName());

        return await FormattedImage.LoadAsync(rawImage, properties.GetFileName());
    }

    private static Type[] GetMigrations()
    {
        return typeof(MigrationExtensions).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(MigrationVersionBase)))
            .ToArray();
    }
}