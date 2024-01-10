using System.Diagnostics;
using System.Reflection;
using Mediator;
using Microsoft.Extensions.Options;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Domain.Constants;
using SF.PhotoPixels.Domain.Repositories;

namespace SF.PhotoPixels.Application.Query.GetStatus;

public class GetStatusHandler : IQueryHandler<GetStatusRequest, QueryResponse<GetStatusResponse>>
{
    private readonly IApplicationConfigurationRepository _configurationRepository;
    private readonly SystemConfig _systemConfig;

    public GetStatusHandler(IApplicationConfigurationRepository configurationRepository, IOptionsMonitor<SystemConfig> systemConfigOptions)
    {
        _configurationRepository = configurationRepository;
        _systemConfig = systemConfigOptions.CurrentValue;
    }

    public async ValueTask<QueryResponse<GetStatusResponse>> Handle(GetStatusRequest request, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        var version = fileVersionInfo.ProductVersion ?? infoVersion ?? fileVersionInfo.FileVersion;

        var appConfig = await _configurationRepository.GetConfiguration();

        return  new GetStatusResponse
        {
            Registration = appConfig.GetValue<bool>(ConfigurationConstants.Registration),
            ServerVersion = version,
            PrivacyTestMode = _systemConfig.PrivacyTestMode ? true : null,
        };
    }
}