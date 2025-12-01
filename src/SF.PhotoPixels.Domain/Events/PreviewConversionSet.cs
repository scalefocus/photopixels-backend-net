namespace SF.PhotoPixels.Domain.Events;

public record PreviewConversionSet
{
    public required Guid UserId { get; init; }
    public required bool PreviewConversion { get; init; }
}
