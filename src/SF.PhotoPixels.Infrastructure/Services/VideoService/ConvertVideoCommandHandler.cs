using SF.PhotoPixels.Domain.Models;
using Wolverine.Attributes;

namespace SF.PhotoPixels.Infrastructure.Services.VideoService;

[LocalQueue("EncodeVideoQueue")]
[RetryNow(typeof(HttpRequestException), 50, 100, 250)]
public class ConvertVideoCommandHandler
{
    public async Task Handle(ConvertVideoCommand command)
    {
        // add some interesting code here...
        Console.WriteLine($"Exec MyLocalCommand with Data='{command.Path}' got added");
        // simulate some work
        await Task.Delay(7000);

        //FormattedVideo.cponvert()

    }
}
