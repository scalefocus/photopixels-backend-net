using Marten;
using Mediator;
using OneOf;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Infrastructure.BackgroundServices;
using SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

namespace SF.PhotoPixels.Application.Commands.Import.StartImport;

public class StartImportHandler : IRequestHandler<StartImportRequest,OneOf< StartImportResponse, ValidationError>>
{
    private readonly IImportDirectoryService _importDirectoryService;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IDocumentSession _session;

    public StartImportHandler(
        IExecutionContextAccessor executionContextAccessor,
        IImportDirectoryService importDirectoryService,
        IDocumentSession documentSession)
    {
        _importDirectoryService = importDirectoryService;
        _executionContextAccessor = executionContextAccessor;
        _session = documentSession;
    }

    public async ValueTask<OneOf<StartImportResponse, ValidationError>> Handle(StartImportRequest request, CancellationToken cancellationToken)
    {

        var user = await _session.LoadAsync<Domain.Entities.User>(_executionContextAccessor.UserId);

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var importTask = new ImportTask()
        {
            Id = Guid.NewGuid(),
            Requester = user,
            Directory = request.ImportDirectoryPath,
            ToDelete = request.DeleteOnImport
        };
        var response = await _importDirectoryService.EnqueueImport(importTask);

        return new StartImportResponse()
        {
            ImportID = importTask.Id,
            QueuePosition = response
        };
    }
}
