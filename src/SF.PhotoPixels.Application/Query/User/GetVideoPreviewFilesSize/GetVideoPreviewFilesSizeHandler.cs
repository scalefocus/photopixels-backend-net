using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Query.User.GetPreviewFilesSize;

public class GetVideoPreviewFilesSizeHandler : IQueryHandler<GetVideoPreviewFilesSizeRequest, OneOf<GetVideoPreviewFilesSizeResponse, NotFound>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IObjectStorage _objectStorage;

    public GetVideoPreviewFilesSizeHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor, IObjectStorage objectStorage)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;
        _objectStorage = objectStorage;
    }

    public async ValueTask<OneOf<GetVideoPreviewFilesSizeResponse, NotFound>> Handle(GetVideoPreviewFilesSizeRequest query, CancellationToken cancellationToken)
    {
        var claimsPrincipal = _executionContextAccessor.UserPrincipal;

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        if (user is null)
        {
            return new NotFound();
        }

        var size = _objectStorage.GetUserConvertedVideosSize(user.Id);

        var userFolders = _objectStorage.GetUserFolders(_executionContextAccessor.UserId);
        var previewVideosFolders = _objectStorage.GetUserConvertedVideosFolder(_executionContextAccessor.UserId);

        var numberOfFilesToConvert = Directory.GetFiles(userFolders.ObjectFolder)
            .Count(f => FormattedVideo.GetExtension(f).ToLower() is "hevc");

        // Build a set of dest file names (without extension, but with "-preview" suffix)
        var numberOfPreviewFiles = Directory.GetFiles(previewVideosFolders).Length;

        return new GetVideoPreviewFilesSizeResponse
        {
            //UserId = user.Id,
            Quota = user.Quota,
            UsedQuota = user.UsedQuota,
            Size = size,
            NumberOfFilesToConvert = numberOfFilesToConvert,
            NumberOfPreviewFiles = numberOfPreviewFiles
        };
    }
}