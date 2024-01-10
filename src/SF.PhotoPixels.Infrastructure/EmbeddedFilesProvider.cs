using System.Reflection;

namespace SF.PhotoPixels.Infrastructure;

public static class EmbeddedFilesProvider
{
    public static string GetEmbeddedFileContent(this Type type, string name)
    {
        return type.Assembly.GetEmbeddedFileContent(name);
    }

    public static string GetEmbeddedFileContent(this Assembly assembly, string name)
    {
        var manifestName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(name));

        if (string.IsNullOrWhiteSpace(manifestName))
        {
            throw new FileNotFoundException($"Embedded file {name} not found in assembly {assembly.FullName}");
        }

        var manifestResourceStream = assembly.GetManifestResourceStream(manifestName);

        using var reader = new StreamReader(manifestResourceStream!);

        return reader.ReadToEnd();
    }
}