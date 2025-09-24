using Mediator;
using OneOf;
using OneOf.Types;
using System.ComponentModel.DataAnnotations;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class DownloadObjectsRequest : IRequest<OneOf<DownloadObjectsResult, NotFound>>
{
    [Required]
    public required List<string> ObjectIds { get; set; }
}

