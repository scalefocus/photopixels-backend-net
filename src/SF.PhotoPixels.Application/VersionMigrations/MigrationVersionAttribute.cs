namespace SF.PhotoPixels.Application.VersionMigrations;

[AttributeUsage(AttributeTargets.Class)]
public class MigrationVersionAttribute : Attribute
{
    public int Version { get; }

    public MigrationVersionAttribute(int version)
    {
        Version = version;
    }
}