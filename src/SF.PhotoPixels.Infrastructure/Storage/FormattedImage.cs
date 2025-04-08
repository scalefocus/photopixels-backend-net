using System.Globalization;
using System.Security.Cryptography;
using SF.PhotoPixels.Domain.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

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

        _image.Mutate(context =>
        {
            context.Resize(new ResizeOptions
            {
                Size = transform,
                Mode = ResizeMode.Max,
            });

            // Reduce the size of the file by clearing metadata
            _image.Metadata.ExifProfile = null;
            _image.Metadata.IptcProfile = null;
            _image.Metadata.XmpProfile = null;
        });

        return new FormattedImage(_image, WebpFormat.Instance);
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
        var exifProfile = _image.Metadata.ExifProfile;

        if (exifProfile is null)
        {
            return null;
        }

        if (!exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var exifValue))
        {
            return null;
        }

        return GetDateTimeInternal(exifValue.Value);
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

        return GetDateTimeInternal(exifValue.Value);
    }

    private DateTimeOffset? GetDateTimeInternal(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
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
}