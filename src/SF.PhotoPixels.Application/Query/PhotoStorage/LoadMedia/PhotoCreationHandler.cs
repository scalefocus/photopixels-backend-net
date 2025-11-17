using HeyRed.ImageSharp.Heif;
using Microsoft.Extensions.Options;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.PrivacyMode;
using SF.PhotoPixels.Domain.Models;
using SF.PhotoPixels.Infrastructure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class PhotoCreationHandler : IMediaCreationHandler
{
    private readonly IObjectStorage _objectStorage;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly SystemConfig _systemConfig;
    private readonly MemoryStream _memoryStream = new();

    public PhotoCreationHandler(IObjectStorage objectStorage,
        IExecutionContextAccessor executionContextAccessor,
        IOptions<SystemConfig> config
        )
    {
        _objectStorage = objectStorage;
        _executionContextAccessor = executionContextAccessor;
        _systemConfig = config.Value;
    }

    public async ValueTask<QueryResponse<LoadMediaResponse>> Handle(LoadMediaCreationModel model, CancellationToken cancellationToken)
    {
        var photo = FormattedVideo.GetExtension(model.FileName.ToLower()) is "heif" or "heic"
            ? await LoadIphoneFormatPhoto(model.FileName, cancellationToken)
            : await LoadPhoto(model.FileName, cancellationToken);

        if (string.IsNullOrWhiteSpace(model.Format))
        {
            return new LoadMediaResponse
            {
                Media = photo,
                ContentType = !_systemConfig.PrivacyTestMode ? model.MimeType! : "image/jpeg",
            };
        }

        var formattedImage = await FormattedImage.LoadAsync(photo, cancellationToken);
        var ms = new MemoryStream();
        await formattedImage.SaveToFormat(ms, model.Format, cancellationToken);

        ms.Seek(0, SeekOrigin.Begin);

        return new LoadMediaResponse
        {
            Media = ms,
            ContentType = formattedImage.GetMimeType() ?? "",
        };
    }

    private async Task<Stream> LoadPhoto(string name, CancellationToken cancellationToken)
    {
        if (!_systemConfig.PrivacyTestMode)
        {
            return await _objectStorage.LoadObjectAsync(_executionContextAccessor.UserId, name, cancellationToken);
        }

        if (_memoryStream.Length == 0)
        {
            PrivacyModeFileLoader.LoadFullImage(_memoryStream);
        }

        _memoryStream.Seek(0, SeekOrigin.Begin);

        return _memoryStream;
    }

    private async Task<Stream> LoadIphoneFormatPhoto(string name, CancellationToken cancellationToken)
    {
        try
        {
            var decoderOptions = new HeifDecoderOptions
            {
                DecodingMode = DecodingMode.TopLevelImages,
                GeneralOptions = new DecoderOptions
                {
                    MaxFrames = 16,
                }
            };

            using var inputStream = await _objectStorage.LoadObjectAsync(_executionContextAccessor.UserId, name, cancellationToken);
            using var heifImage = HeifDecoder.Instance.Decode(decoderOptions, inputStream);

            var frameCount = heifImage.Frames.Count;
            if (frameCount == 0)
            {
                throw new InvalidDataException("HEIC/HEIF image contains no frames.");
            }

            (float cellScale, int gridRows, int gridCols) = GetGridDimension(frameCount);

            var frameWidth = heifImage.Frames[0].Width;
            var frameHeight = heifImage.Frames[0].Height;

            var cellWidth = (int)(frameWidth * cellScale);
            var cellHeight = (int)(frameHeight * cellScale);

            using var outputImage = new Image<Rgba32>(gridCols * cellWidth, gridRows * cellHeight);

            for (var i = 0; i < Math.Min(frameCount, gridCols * gridRows); i++)
            {
                using var frame = heifImage.Frames.CloneFrame(i);

                // Resize frame if needed
                if (cellScale != 1f)
                {
                    frame.Mutate(ctx => ctx.Resize(cellWidth, cellHeight));
                }

                var x = (i % gridCols) * cellWidth;
                int y = (i / gridCols) * cellHeight;
                outputImage.Mutate(ctx => ctx.DrawImage(frame, new Point(x, y), 1f));
            }

            _memoryStream.SetLength(0);
            await outputImage.SaveAsJpegAsync(_memoryStream, cancellationToken: cancellationToken);
            _memoryStream.Seek(0, SeekOrigin.Begin);

            return _memoryStream;
        }
        catch (Exception ex)
        {
            var eex = ex;
            throw new InitializationException("Failed to load HEIC/HEIF image.", ex);
        }
    }

    static (float cellScale, int gridRows, int gridCols) GetGridDimension(int frameCount)
    {
        float cellScale = 1f;
        int gridCols, gridRows;
        switch (frameCount)
        {
            case <= 1:
                gridCols = 1;
                gridRows = 1;
                break;
            case <= 2:
                gridCols = 2;
                gridRows = 1;
                break;
            case <= 4:
                gridCols = 2;
                gridRows = 2;
                cellScale = 1.1f;
                break;
            case <= 6:
                gridCols = 3;
                gridRows = 2;
                cellScale = 1.2f;
                break;
            case <= 9:
                gridCols = 3;
                gridRows = 3;
                cellScale = 1.4f;
                break;
            default:
                gridCols = 4;
                gridRows = 4;
                cellScale = 1.5f;
                break;
        }
        return (cellScale, gridRows, gridCols);
    }
}
