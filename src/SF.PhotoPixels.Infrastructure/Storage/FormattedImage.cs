using HeyRed.ImageSharp.Heif;
using SF.PhotoPixels.Domain.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Security.Cryptography;

namespace SF.PhotoPixels.Infrastructure.Storage;

public sealed class FormattedImage : IDisposable, IStorageItem
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IImageFormat _format;
    private readonly Image _image;

    private byte[]? _hash;
    private readonly ImageFormatManager _imageFormatsManager;

    public int Width => _image.Width;

    public int Height => _image.Height;

    private FormattedImage(Image image, IImageFormat format)
    {
        _image = image;
        _format = format;
        _imageFormatsManager = image.Configuration.ImageFormatsManager;
    }

    public void Dispose()
    {
        _image.Dispose();
    }

    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return _image.SaveAsync(stream, _imageFormatsManager.GetEncoder(_format), cancellationToken);
    }

    public Task SaveToFormat(Stream stream, string requestedFormat, CancellationToken cancellationToken = default)
    {
        var format = _imageFormatsManager.ImageFormats.FirstOrDefault(x => x.Name.Equals(requestedFormat, StringComparison.OrdinalIgnoreCase));

        if (format is null)
        {
            throw new Exception();
        }

        return _image.SaveAsync(stream, _imageFormatsManager.GetEncoder(format), cancellationToken);
    }

    public static async Task<FormattedImage> LoadAsync(Stream source, CancellationToken cancellationToken = default)
    {
        var image = await Image.LoadAsync(source, cancellationToken);

        return new FormattedImage(image, image.Metadata.DecodedImageFormat!);
    }

    public FormattedImage GetThumbnail(int width = 160, int height = 160)
    {
        var transform = Transform(new Size(width, height));
        var newimage = _image.Clone(context => context.Resize(new ResizeOptions
        {
            Size = transform,
            Mode = ResizeMode.Max,
        }));

        newimage.Mutate(context =>
        {
            context.Resize(new ResizeOptions
            {
                Size = transform,
                Mode = ResizeMode.Max,
            });

            // Reduce the size of the file by clearing metadata
            newimage.Metadata.ExifProfile = null;
            newimage.Metadata.IptcProfile = null;
            newimage.Metadata.XmpProfile = null;
        });

        return new FormattedImage(newimage, WebpFormat.Instance);
    }

    public DateTimeOffset GetDateTime()
    {
        var datetime = GetOriginalDateTime();

        if (datetime is not null)
        {
            return datetime.Value;
        }

        datetime = GetCreatedDateTime();

        if (datetime is not null)
        {
            return datetime.Value;
        }

        return DateTime.UtcNow;
    }

    public DateTimeOffset? GetOriginalDateTime()
    {
        //_image.Frames[0].Metadata.ExifProfile
        var exifProfile = _image.Metadata.ExifProfile;

        if (exifProfile is null)
        {
            return null;
        }

        if (!exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeOriginal))
        {
            return null;
        }

        exifProfile.TryGetValue(ExifTag.OffsetTimeOriginal, out var offsetTimeOriginal);

        return GetDateTimeInternal(dateTimeOriginal.Value, offsetTimeOriginal?.Value);
    }

    public DateTimeOffset? GetCreatedDateTime()
    {
        var exifProfile = _image.Metadata.ExifProfile;

        if (exifProfile is null)
        {
            return null;
        }

        if (!exifProfile.TryGetValue(ExifTag.DateTime, out var exifValue))
        {
            return null;
        }

        exifProfile.TryGetValue(ExifTag.OffsetTimeOriginal, out var offsetTimeOriginal);

        return GetDateTimeInternal(exifValue.Value, offsetTimeOriginal?.Value);
    }

    private DateTimeOffset? GetDateTimeInternal(string? dateString, string? offsetTimeOriginal)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (!DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
        {
            return null;
        }

        // If the offset is provided, parse it and combine with the date
        if (!string.IsNullOrWhiteSpace(offsetTimeOriginal))
        {
            var offset = TimeSpan.Parse(offsetTimeOriginal.Substring(1), CultureInfo.InvariantCulture);
            return offsetTimeOriginal.StartsWith('-')
                    ? new DateTimeOffset(dateTime, -offset)
                    : new DateTimeOffset(dateTime, offset);
        }

        if (DateTimeOffset.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTimeOffset))
        {
            return dateTimeOffset.ToUniversalTime();
        }

        if (DateTime.TryParse(dateString, out var value))
        {
            return value.ToUniversalTime();
        }

        return null;
    }

    public int GetExifOrientation()
    {
        if (_image.Metadata.ExifProfile != null)
        {
            if (!_image.Metadata.ExifProfile.TryGetValue(ExifTag.Orientation, out var orientation))
            {
                return ExifOrientationMode.Unknown;
            }

            if (orientation.DataType == ExifDataType.Short)
            {
                return orientation.Value;
            }

            return Convert.ToUInt16(orientation.Value);
        }
        return ExifOrientationMode.Unknown;
    }

    public bool IsExifOrientationRotated()
    {
        return GetExifOrientation() switch
        {
            ExifOrientationMode.LeftTop
                or ExifOrientationMode.RightTop
                or ExifOrientationMode.RightBottom
                or ExifOrientationMode.LeftBottom => true,
            _ => false,
        };
    }

    private Size Transform(Size size)
    {
        var s = IsExifOrientationRotated()
            ? new Size(size.Height, size.Width)
            : size;

        if (_image.Width > _image.Height)
        {
            return new Size(0, s.Height);
        }

        if (_image.Height > _image.Width)
        {
            return new Size(s.Width, 0);
        }

        return s;
    }

    public string GetExtension()
    {
        return _format.FileExtensions.First();
    }

    public string? GetMimeType()
    {
        var imageFormat = _image.Metadata.DecodedImageFormat;

        return imageFormat?.DefaultMimeType;
    }

    public async Task<string> GetSafeFingerprintAsync()
    {
        return Base64Url.Encode(await GetHashAsync());
    }

    public async Task<byte[]> GetHashAsync()
    {
        if (_hash is not null)
        {
            return _hash;
        }

        using var ms = new MemoryStream();

        await _image.SaveAsync(ms, new JpegEncoder());

        ms.Seek(0, SeekOrigin.Begin);

        return _hash = await SHA1.HashDataAsync(ms);
    }

    public static async Task<Stream> LoadHeifAsync(FileStream inputStream, CancellationToken cancellationToken, MemoryStream? memoryStream = null)
    {
        var decoderOptions = new HeifDecoderOptions
        {
            DecodingMode = DecodingMode.TopLevelImages,
            GeneralOptions = new DecoderOptions
            {
                MaxFrames = 16,
            }
        };

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

        // Ensure memoryStream is not null before dereferencing
        memoryStream ??= new MemoryStream();
        memoryStream.SetLength(0);
        await outputImage.SaveAsJpegAsync(memoryStream, cancellationToken: cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private static (float cellScale, int gridRows, int gridCols) GetGridDimension(int frameCount)
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