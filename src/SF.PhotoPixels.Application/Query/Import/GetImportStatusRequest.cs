using Mediator;

namespace SF.PhotoPixels.Application.Query.Import;

public class GetImportStatusRequest : IQuery<GetImportStatusResponse>
{
    public required Guid Id { get; set; }
}
