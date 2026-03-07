using Aspire.Hosting.Publishing;

namespace SF.PhotoPixels.Aspire;

public class EmptyStringParameterDefault : ParameterDefault
{
    public static EmptyStringParameterDefault Instance => LazyInstance.Value;
    private static readonly Lazy<EmptyStringParameterDefault> LazyInstance = new(() => new EmptyStringParameterDefault());

    private EmptyStringParameterDefault()
    {
    }

    public override string GetDefaultValue()
    {
        return string.Empty;
    }

    public override void WriteToManifest(ManifestPublishingContext context)
    {
        context.Writer.WriteString("value", GetDefaultValue());
    }
}