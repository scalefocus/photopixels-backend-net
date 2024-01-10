using Mediator;
using SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

namespace SF.PhotoPixels.Application.Query.Import;

public class GetImportStatusHandler : IQueryHandler<GetImportStatusRequest, GetImportStatusResponse>
{
    private readonly IImportDirectoryService _importDirectoryService;

    public GetImportStatusHandler(IImportDirectoryService importDirectoryService)
    {
        _importDirectoryService = importDirectoryService;
    }

    public async ValueTask<GetImportStatusResponse> Handle(GetImportStatusRequest query, CancellationToken cancellationToken)
    {
        ImportTaskProgress progress = await _importDirectoryService.GetProgress(query.Id);
        if(progress is null)
        {
            return new GetImportStatusResponse();
        }

        return new GetImportStatusResponse()
        {
            ImportedItems = progress.ProcessedFiles,
            TotalItems = progress.TotalFiles,
            DuplicateItems = progress.DuplicateFiles,
            IsInQueue = progress.IsInQueue
        };
    }
}
