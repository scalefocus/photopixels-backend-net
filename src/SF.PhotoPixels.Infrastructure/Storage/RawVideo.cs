using System.Security.Cryptography;
using SF.PhotoPixels.Domain.Utils;

namespace SF.PhotoPixels.Infrastructure.Storage;

public class RawVideo : IDisposable, IStorageItem
{
    private readonly Stream _rawStream;
    private FormattedVideo? _formattedVideo;
    private readonly string _filename;

    private byte[]? _hash;

    public string GetFileName() => _filename;

    public RawVideo(Stream stream, string filename)
    {
        _rawStream = new MemoryStream((int)stream.Length);
        _filename = filename;
        stream.CopyTo(_rawStream);
    }

    public void Dispose()
    {
        _formattedVideo?.Dispose();
        _rawStream.Dispose();
    }

    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        _rawStream.Position = 0;
        return _rawStream.CopyToAsync(stream, cancellationToken);
    }

    public async Task<byte[]> GetHashAsync()
    {
        if (_hash is not null)
        {
            return _hash;
        }

        _rawStream.Seek(0, SeekOrigin.Begin);

        return _hash = await SHA1.HashDataAsync(_rawStream);
    }

    public async Task<string> GetSafeFingerprintAsync()
    {
        return Base64Url.Encode(await GetHashAsync());
    }

    public async Task<FormattedVideo> ToFormattedVideoAsync(string name, CancellationToken cancellationToken = default)
    {
        await EnsureFormattedVideoLoaded(name, cancellationToken);

        return _formattedVideo!;
    }

    public long GetVideoSize()
    {
        return _rawStream.Length;
    }

    private async Task EnsureFormattedVideoLoaded(string name, CancellationToken cancellationToken = default)
    {
        if (_formattedVideo is not null)
        {
            return;
        }

        _formattedVideo = await FormattedVideo.LoadAsync(name, cancellationToken);
    }
}