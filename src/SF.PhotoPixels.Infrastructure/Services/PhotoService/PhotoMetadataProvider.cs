using SF.PhotoPixels.Domain.Models;
using System.Globalization;

namespace SF.PhotoPixels.Infrastructure.Services.PhotoService;

public class PhotoMetadataProvider
{
    public static MetadataDates GetMetadataDates(string path)
    {
        MetadataDates metadataDates = new();
        using var magickImage = new ImageMagick.MagickImage(path);

        var profile = magickImage!.GetExifProfile();

        // datePhotopixelsImported
        DateTime dateCreate = DateTime.Now;
        if (DateTime.TryParse(magickImage.GetAttribute("date:create"), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var exifDate))
        {
            dateCreate = exifDate;
        }

        metadataDates.datePhotopixelsImported = GetDateTimeInternal(dateCreate.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture), null);

        if (profile != null)
        {
            // dateMediaTaken
            var dateTimeOriginalValue = profile.GetValue(ImageMagick.ExifTag.DateTimeOriginal);
            var dateTimeOriginalOffsetValue = profile.GetValue(ImageMagick.ExifTag.OffsetTimeOriginal);
            metadataDates.dateMediaTaken = GetDateTimeInternal(dateTimeOriginalValue?.Value, dateTimeOriginalOffsetValue?.Value);

            // dateMediaCreated
            var dateTimeValue = profile.GetValue(ImageMagick.ExifTag.DateTime);
            var dateTimeOffsetValue = profile.GetValue(ImageMagick.ExifTag.OffsetTime);
            metadataDates.dateMediaCreated = GetDateTimeInternal(dateTimeValue?.Value, dateTimeOffsetValue?.Value) ?? metadataDates.datePhotopixelsImported;
        }

        //// datePhoneImported
        //var dateTimeOriginalValue = profile.GetValue(ImageMagick.ExifTag.DateTimeOriginal);
        //var dateTimeOriginalOffsetValue = profile.GetValue(ImageMagick.ExifTag.OffsetTimeOriginal);
        //metadataDates.dateMediaTaken = GetDateTimeInternal(dateTimeOriginalValue.Value, dateTimeOriginalOffsetValue?.Value);

        //// dateMediaModified
        //var dateTimeOriginalValue = profile.GetValue(ImageMagick.ExifTag.DateTimeOriginal);
        //var dateTimeOriginalOffsetValue = profile.GetValue(ImageMagick.ExifTag.OffsetTimeOriginal);
        //metadataDates.dateMediaTaken = GetDateTimeInternal(dateTimeOriginalValue.Value, dateTimeOriginalOffsetValue?.Value);

        return metadataDates;
    }

    public static DateTimeOffset? GetDateTimeInternal(string? dateString, string? offsetTimeOriginal)
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
}
