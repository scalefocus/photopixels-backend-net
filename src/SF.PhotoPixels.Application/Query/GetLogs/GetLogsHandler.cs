using Mediator;
using OneOf;
using OneOf.Types;
using System.Runtime.InteropServices;

namespace SF.PhotoPixels.Application.Query.GetLogs;

public class GetLogsHandler : IQueryHandler<GetLogsRequest, OneOf<Stream, None>>, IDisposable, IAsyncDisposable
{
    private readonly MemoryStream _memoryStream = new();

    public async ValueTask DisposeAsync()
    {
        await _memoryStream.DisposeAsync();
    }

    public void Dispose()
    {
        _memoryStream.Dispose();
    }
    public async ValueTask<OneOf<Stream, None>> Handle(GetLogsRequest request, CancellationToken cancellationToken)
    {
        DateTime today = DateTime.Today;
        string formattedDate = today.ToString("yyyyMMdd");

        var logFilePath = Path.Combine(GetRootDirectory(), "photopixels", "log-" + formattedDate + ".log");

        if (File.Exists(logFilePath))
        {
            var copyPath = logFilePath.Replace(formattedDate, formattedDate + "Copy");
            File.Copy(logFilePath, copyPath);

            string logText = await File.ReadAllTextAsync(copyPath, cancellationToken);
            File.Delete(copyPath);

            await _memoryStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(logText), cancellationToken);
            _memoryStream.Seek(0, SeekOrigin.Begin);

            return _memoryStream;

        }
        return new None();
    }
    private static string GetRootDirectory()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine(Path.DirectorySeparatorChar + "var", "log") : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}