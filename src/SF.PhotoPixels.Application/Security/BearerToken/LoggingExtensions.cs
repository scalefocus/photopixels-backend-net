// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace SF.PhotoPixels.Application.Security.BearerToken;
internal static partial class LoggingExtensions
{
    [LoggerMessage(1, LogLevel.Information, "AuthenticationScheme: {AuthenticationScheme} signed in.", EventName = "AuthenticationSchemeSignedIn")]
    public static partial void AuthenticationSchemeSignedIn(this ILogger logger, string authenticationScheme);
}