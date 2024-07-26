using System.Diagnostics.CodeAnalysis;
using Mediator;

namespace SF.PhotoPixels.Application
{
    public interface IValidate : IMessage
    {
        bool IsValid([NotNullWhen(false)] out ValidationError? error);
    }
}
