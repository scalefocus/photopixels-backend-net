namespace SF.PhotoPixels.Domain.Entities;

public class Album
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<string> ObjectPropertiesIds { get; set; } = new();
    public Guid UserId { get; set; }

}
