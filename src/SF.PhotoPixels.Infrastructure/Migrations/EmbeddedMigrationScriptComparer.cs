namespace SF.PhotoPixels.Infrastructure.Migrations;

public class EmbeddedMigrationScriptComparer : IComparer<string>
{
    private const string MigrationsFolderName = "Migrations";

    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        return string.Compare(GetScriptName(x), GetScriptName(y), StringComparison.Ordinal);
    }

    private static string GetScriptName(string fullName)
    {
        var migrationsFolderIndex = fullName.IndexOf(MigrationsFolderName, StringComparison.OrdinalIgnoreCase) + MigrationsFolderName.Length + 1;

        return fullName[migrationsFolderIndex..];
    }
}