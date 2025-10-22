namespace SF.PhotoPixels.Domain.Entities;

public class Album
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required bool IsSystem { get; set; }
    public required DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    public Guid UserId { get; set; }
}