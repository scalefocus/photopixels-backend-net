
namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjectsTrashed;

public class GetObjectsTrashedResponse
{
    public required string LastId { get; set; }    
    public required List<PropertiesTrashedResponse> Properties { get; set; }
}
