using OneOf;

namespace SF.PhotoPixels.Application;

[GenerateOneOf]
public sealed record ValidationError(IDictionary<string, string[]> Errors)
{
    public ValidationError(string code, string message) : this(new Dictionary<string, string[]> { { code, new[] { message } } })
    {
    }
}