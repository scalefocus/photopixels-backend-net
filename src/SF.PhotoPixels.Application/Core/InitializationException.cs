using System.Runtime.Serialization;

namespace SF.PhotoPixels.Application.Core;

public class InitializationException : Exception
{
    public InitializationException()
    {
    }

    protected InitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InitializationException(string? message) : base(message)
    {
    }

    public InitializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}