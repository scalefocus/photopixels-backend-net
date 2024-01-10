using System.Text;

namespace SF.PhotoPixels.Domain.Utils;

public static class Base64Url
{
    public static byte[] Decode(string encoded)
    {
        encoded = new StringBuilder(encoded)
            .Replace('-', '+')
            .Replace('_', '/')
            .Append('=', encoded.Length % 4 == 0 ? 0 : 4 - (encoded.Length % 4))
            .ToString();

        return Convert.FromBase64String(encoded);
    }

    public static string Encode(byte[] bytes)
    {
        return new StringBuilder(Convert.ToBase64String(bytes)).Replace('+', '-').Replace('/', '_').Replace("=", "").ToString();
    }

    public static string DecodeString(string encoded)
    {
        return Encoding.UTF8.GetString(Decode(encoded));
    }

    public static string EncodeString(string value)
    {
        return Encode(Encoding.UTF8.GetBytes(value));
    }
}