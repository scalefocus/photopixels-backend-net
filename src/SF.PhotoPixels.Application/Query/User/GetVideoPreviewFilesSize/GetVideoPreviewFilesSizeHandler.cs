using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;
using SF.PhotoPixels.Infrastructure.Storage;
using System.Runtime.InteropServices;

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

        var userFolders = _objectStorage.GetUserFolders(_executionContextAccessor.UserId);
        var previewVideosFolders = _objectStorage.GetUserConvertedVideosFolder(_executionContextAccessor.UserId);

        // check if still some files remain converted
        var toDeletePreviewFiles = false;
        var size = _objectStorage.GetUserConvertedVideosSize(user.Id);
        if (!user.Settings.AllowVideoConversion && size > 0)
        {
            toDeletePreviewFiles = true;
        }

        var filesToConvert = new DirectoryInfo(userFolders.ObjectFolder)
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(f => f.Extension.Equals(".hevc", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var numberOfFilesToConvert = filesToConvert.Count;
        var sizeOfFilesToConvert = filesToConvert.Sum(fi => fi.Length);

        var previewFiles = Directory.GetFiles(previewVideosFolders);
        var numberOfPreviewFiles = 0;
        foreach (var file in previewFiles) {    
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            var originalFileName = fileNameWithoutExtension.Replace("-preview", "");
            var tempDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                "/tmp" :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sf-photos", "temp");
            var tempFilePath = Path.Combine(tempDirectory, originalFileName + ".hevc");
            if (!File.Exists(tempFilePath))
            {
                //if exist in temp foolder then it is in conversation process and we should not count it as preview file
                numberOfPreviewFiles++;
            }
        }

        return new GetVideoPreviewFilesSizeResponse
        {
            Quota = user.Quota,
            UsedQuota = user.UsedQuota,
            Size = size,
            NumberOfFilesToConvert = numberOfFilesToConvert,
            NumberOfPreviewFiles = numberOfPreviewFiles,
            SizeOfFilesToConvert = sizeOfFilesToConvert,
            ToDeletePreviewFiles = toDeletePreviewFiles
        };
    }
}