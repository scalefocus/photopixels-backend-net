namespace SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;

public class GetObjectsResponse
{
    public required string LastId { get; set; }    
    public required List<PropertiesResponse> Properties { get; set; }
}
