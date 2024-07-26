using System.Net;

namespace SF.PhotoPixels.Domain.Exceptions;

public class FailRequestException : Exception
{
    public HttpStatusCode StatusCode { get; set; }

    public FailRequestException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
