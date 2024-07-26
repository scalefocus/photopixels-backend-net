using SolidTUS.Models;

namespace SF.PhotoPixels.Infrastructure.Services.TusService;

public interface ITusService
{
    Task HandleNewCompletion(UploadFileInfo fileInfo);
}