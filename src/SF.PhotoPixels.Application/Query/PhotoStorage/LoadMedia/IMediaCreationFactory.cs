namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public interface IMediaCreationFactory
{
    public IMediaCreationHandler CreateMediaHandler(string extension);
}

