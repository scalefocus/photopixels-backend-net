using Microsoft.Extensions.Logging;

namespace SF.PhotoPixels.Application.VersionMigrations;

internal static partial class VersionMigrations
{
    private const int EventBaseId = 10000;

    internal static partial class Log
    {
        [LoggerMessage(EventBaseId + 1, LogLevel.Information, "Total objects to process: {TotalObjectsCount}.", EventName = "TotalObjectsToProcess")]
        public static partial void TotalObjectsToProcess(ILogger logger, int totalObjectsCount);

        [LoggerMessage(EventBaseId + 2, LogLevel.Information, "Executing migration for object {ObjectName} for the user {User}", EventName = "ObjectMigrationExecutionStarting")]
        public static partial void ObjectMigrationExecutionStarting(ILogger logger, string objectName, string user);

        [LoggerMessage(EventBaseId + 3, LogLevel.Information, "Migration for object {ObjectName} for the user {User} completed", EventName = "ObjectMigrationExecutionFinished")]
        public static partial void ObjectMigrationExecutionFinished(ILogger logger, string objectName, string user);

        [LoggerMessage(EventBaseId + 4, LogLevel.Information, "Processed objects: {ProcessedObjectsCount}/{TotalObjectsCount}.", EventName = "TotalObjectsProcessed")]
        public static partial void TotalObjectsProcessed(ILogger logger, int processedObjectsCount, int totalObjectsCount);
    }
}