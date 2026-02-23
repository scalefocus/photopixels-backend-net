using SF.PhotoPixels.Application.Commands.VideoStorage;
using SF.PhotoPixels.Domain.Models;
using System.Text.Json;
using Wolverine;

namespace SF.PhotoPixels.API.Middlewares;

public static class StopProcessingMiddleware
{
    // Wolverine looks for "Before" methods by convention
    public static HandlerContinuation Before(ConvertVideoCommand command)
    {
        if (UserCancellationStore.IsUserCancelled(command.UserId))
        {
            // Tells Wolverine to abort this message immediately
            return HandlerContinuation.Stop;
        }

        return HandlerContinuation.Continue;
    }
}
