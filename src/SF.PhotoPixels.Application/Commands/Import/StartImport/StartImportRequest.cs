using Mediator;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.Import.StartImport;

public class StartImportRequest : IRequest<OneOf<StartImportResponse, ValidationError>>
{
    public required string ImportDirectoryPath {  get; set; }

    public bool DeleteOnImport {  get; set; } = false;
}
