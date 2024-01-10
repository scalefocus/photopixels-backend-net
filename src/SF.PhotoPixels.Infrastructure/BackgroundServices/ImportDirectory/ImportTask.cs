using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;

public class ImportTask
{
    public Guid Id { get; set; }

    public User Requester { get; set; }

    public string Directory { get; set; }

    public bool ToDelete { get; set; }

}
