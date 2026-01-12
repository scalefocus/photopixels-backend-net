using System.Security.Cryptography;
using SF.PhotoPixels.Domain.Utils;

namespace SF.PhotoPixels.Infrastructure.Storage;

public class RawImage : IDisposable, IStorageItem
{
    private readonly Stream _rawStream;
    private FormattedImage? _formattedImage;
    private readonly string _filename;

    private byte[]? _hash;

    public string GetFileName() => _filename;
    public Stream GetStream() => _rawStream;

    public RawImage(Stream stream, string filename)
    {
        _rawStream = new MemoryStream((int)stream.Length);
        _filename = filename;
        stream.CopyTo(_rawStream);
    }

    public void Dispose()
    {
        _formattedImage?.Dispose();
        _rawStream.Dispose();
    }

    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        _rawStream.Position = 0;

        return _rawStream.CopyToAsync(stream, cancellationToken);
    }

    public async Task<FormattedImage> ToFormattedImageAsync(CancellationToken cancellationToken = default)
    {
        await EnsureFormattedImageLoaded(cancellationToken);

        return _formattedImage!;
    }

    public long GetImageSize()
    {
        return _rawStream.Length;
    }

    public async Task<string> GetSafeFingerprintAsync()
    {
        return Base64Url.Encode(await GetHashAsync());
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

    private async Task EnsureFormattedImageLoaded(CancellationToken cancellationToken = default)
    {
        if (_formattedImage is not null)
        {
            return;
        }

        _rawStream.Seek(0, SeekOrigin.Begin);

        _formattedImage = await FormattedImage.LoadAsync(_rawStream, _filename, cancellationToken);
    }
}