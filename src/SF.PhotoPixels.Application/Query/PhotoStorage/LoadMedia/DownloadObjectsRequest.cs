using System.ComponentModel.DataAnnotations;
using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class DownloadObjectsRequest : IRequest<OneOf<byte[], NotFound>>
{
    [Required]
    public required List<string> ObjectIds { get; set; }
}

