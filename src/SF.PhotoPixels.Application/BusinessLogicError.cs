using OneOf;

namespace SF.PhotoPixels.Application;

[GenerateOneOf]
public sealed record BusinessLogicError(IDictionary<string, string[]> Errors)
{
    public BusinessLogicError(string code, string message) : this(new Dictionary<string, string[]> { { code, new[] { message } } })
    {
    }    
}