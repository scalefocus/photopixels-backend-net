using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Commands.VideoStorage;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Storage;
using Wolverine;

namespace SF.PhotoPixels.Application.Commands.User.AllowVideoConversion;

public class AllowVideoConversionHandler : IRequestHandler<AllowVideoConversionRequest, OneOf<Success, ValidationError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IObjectStorage _objectStorage;
    private readonly IMessageBus _bus;

    public AllowVideoConversionHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor, IObjectStorage objectStorage, IMessageBus bus)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;
        _objectStorage = objectStorage;
        _bus = bus;
    }

    public async ValueTask<OneOf<Success, ValidationError>> Handle(AllowVideoConversionRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_executionContextAccessor.UserId.ToString());

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        user.Settings.AllowVideoConversion = request.AllowVideoConversion;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return new ValidationError("UserUpdateFailed", errorMessage);
        }

        if (!request.AllowVideoConversion)
        {
            UserCancellationStore.AddUser(_executionContextAccessor.UserId, DateTime.UtcNow);
            return new Success();
        }

        UserCancellationStore.RemoveUser(_executionContextAccessor.UserId);
        await ConvertedFilesAsync();

        return new Success();
    }

    public async Task ConvertedFilesAsync()
    {
        // Ensure destination directory exists
        var userFolders = _objectStorage.GetUserFolders(_executionContextAccessor.UserId);
        var previewVideosFolders = _objectStorage.GetUserConvertedVideosFolder(_executionContextAccessor.UserId);

        // Get all files in source directory, filter by supported video formats
        var sourceFiles = Directory.GetFiles(userFolders.ObjectFolder)
            .Where(f => FormattedVideo.GetExtension(f).ToLower() is "hevc");

        // Build a set of dest file names (without extension, but with "-preview" suffix)
        var destFiles = new HashSet<string>(
            Directory.GetFiles(previewVideosFolders)
                .Select(f => Path.GetFileNameWithoutExtension(f)), StringComparer.OrdinalIgnoreCase);

        foreach (var sourceFile in sourceFiles)
        {
            if (!destFiles.Contains($"{Path.GetFileNameWithoutExtension(sourceFile)}{Infrastructure.Constants.PreviewSufix}"))
            {
                var command = new ConvertVideoCommand
                {
                    Filename = Path.GetFileName(sourceFile),
                    objectPath = userFolders.ObjectFolder,
                    convertedVideoPath = previewVideosFolders,
                    thumbVideoPath = userFolders.ThumbFolder,
                    UserId = _executionContextAccessor.UserId
                };
                await _bus.PublishAsync(command);
            }
        }
    }
}
