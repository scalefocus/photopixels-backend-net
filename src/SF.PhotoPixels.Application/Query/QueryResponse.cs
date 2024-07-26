using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query;

[GenerateOneOf]
public partial class QueryResponse<T> : OneOfBase<T, NotFound>
{
}