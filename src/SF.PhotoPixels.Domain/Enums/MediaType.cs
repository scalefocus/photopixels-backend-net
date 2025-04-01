using System.ComponentModel;

namespace SF.PhotoPixels.Domain.Enums;

public enum MediaType
{
    [Description("Unknown")]
    Unknown,
    [Description("Photo")]
    Photo,
    [Description("Video")]
    Video,
}