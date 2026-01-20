using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure.Services.PhotoService;
using SF.PhotoPixels.Infrastructure.Storage;
using System.Collections.Concurrent;

namespace SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

public class ImportDirectoryService : BackgroundService, IImportDirectoryService
{
    private static readonly ConcurrentDictionary<Guid, ImportTask> _importQueue = new();
    private static readonly SemaphoreSlim _lock = new(0);
    private static ImportTask? _currentTask;
    private static ImportTaskProgress? _currentProgress;
    private List<string> _filesToDelete;

    private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".heic" };
    private readonly ILogger<ImportDirectoryService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ImportDirectoryService(
        ILogger<ImportDirectoryService> logger,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<ImportTaskProgress?> GetProgress(Guid id)
    {
        ImportTaskProgress? toReturn = null;
        if (_importQueue.ContainsKey(id))
        {
            toReturn = new ImportTaskProgress() { IsInQueue = true };
        }

        if (_currentTask is not null && _currentTask.Id.Equals(id))
        {
            toReturn = _currentProgress;
        }

        return Task.FromResult(toReturn);
    }

    public Task<int> EnqueueImport(ImportTask task)
    {
        var result = _importQueue.TryAdd(task.Id, task);
        if (result) _lock.Release();

        return Task.FromResult(_importQueue.Count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await _lock.WaitAsync(stoppingToken);

            if (!_importQueue.IsEmpty)
            {
                try
                {
                    await ProcessImportRequest(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }

    private async Task ProcessImportRequest(CancellationToken cancellationToken)
    {
        _currentTask = _importQueue.First().Value;
        _importQueue.TryRemove(_importQueue.First());
        _filesToDelete = new List<string>();
        _currentProgress = new ImportTaskProgress()
        {
            IsInQueue = false
        };

        _logger.LogInformation("Starting import with id {id}", _currentTask!.Id);

        var canRead = await CanReadDirectory();
        if (!canRead)
        {
            throw new Exception("CantReadDirectory");
        }

        var files = await GetCompatibleFiles();

        _currentProgress.TotalFiles = files.Count();

        await ImportFiles(files, cancellationToken);

        if (_currentTask.ToDelete)
        {
            await DeleteImportedFiles();
        }

    }

    private Task<bool> CanReadDirectory()
    {
        return Task.FromResult(Directory.Exists(_currentTask!.Directory));
    }

    private Task<IEnumerable<string>> GetCompatibleFiles()
    {
        var files = Directory.EnumerateFiles(_currentTask!.Directory).Where(f => SupportedFormats.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

        return Task.FromResult(files);
    }

    private async Task ImportFiles(IEnumerable<string> files, CancellationToken stoppingToken)
    {
        var user = _currentTask!.Requester;
        await using var scope = _serviceProvider.CreateAsyncScope();

        var documentSession = scope.ServiceProvider.GetService<IDocumentSession>();
        var photoService = scope.ServiceProvider.GetService<IPhotoService>();

        foreach (var file in files)
        {
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var rawImage = new RawImage(stream, file);
            var imageFingerprint = await rawImage.GetSafeFingerprintAsync();
            var objectId = new ObjectId(user.Id, imageFingerprint);

            if (await documentSession!.Query<ObjectProperties>().AnyAsync(x => x.Id == objectId, stoppingToken))
            {
                _currentProgress!.DuplicateFiles++;
                continue;
            }

            var usedQuota = await photoService!.SaveFile(rawImage, user.Id, stoppingToken);
            if (!user.IncreaseUsedQuota(usedQuota))
            {
                throw new Exception("MaxQuotaReached");
            }

            documentSession.Update(user);
            await documentSession.SaveChangesAsync(stoppingToken);

            var version = await photoService.StoreObjectCreatedEventAsync(rawImage, usedQuota, file, user.Id, stoppingToken);

            _currentProgress!.ProcessedFiles++;

            if (_currentTask!.ToDelete)
            {
                _filesToDelete.Add(file);
            }
        }
    }

    private Task DeleteImportedFiles()
    {
        _logger.LogInformation("Deleting {filecount} Imported Files", _filesToDelete.Count);
        _filesToDelete.ForEach(File.Delete);
        _logger.LogInformation($"Deletion Complete");

        return Task.CompletedTask;
    }
}
