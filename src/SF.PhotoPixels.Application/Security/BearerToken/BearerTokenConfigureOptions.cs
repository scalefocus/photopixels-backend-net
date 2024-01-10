// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace SF.PhotoPixels.Application.Security.BearerToken;

internal sealed class BearerTokenConfigureOptions : IConfigureNamedOptions<BearerTokenOptions>
{
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public BearerTokenConfigureOptions(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    private const string PrimaryPurpose = "Microsoft.AspNetCore.Authentication.BearerToken";

    public void Configure(string? schemeName, BearerTokenOptions options)
    {
        if (schemeName is null)
        {
            return;
        }

        options.BearerTokenProtector = new TicketDataFormat(_dataProtectionProvider.CreateProtector(PrimaryPurpose, schemeName, "BearerToken"));
        options.RefreshTokenProtector = new TicketDataFormat(_dataProtectionProvider.CreateProtector(PrimaryPurpose, schemeName, "RefreshToken"));
    }

    public void Configure(BearerTokenOptions options)
    {
        throw new NotImplementedException();
    }
}