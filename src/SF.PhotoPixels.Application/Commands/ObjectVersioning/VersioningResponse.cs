using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning;

public class VersioningResponse
{
    public long Revision { get; set; }
}

[GenerateOneOf]
public partial class ObjectVersioningResponse : OneOfBase<VersioningResponse, NotFound>
{
}