using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

internal static class HostingExtensionsLogging
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        McpServerSettings settings = builder.Configuration
                                            .GetSection("McpServer")
                                            .Get<McpServerSettings>() ?? new();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        if (settings.EnableApplicationInsights &&
            !string.IsNullOrWhiteSpace(settings.ApplicationInsightsConnectionString))
        {
            builder.Logging.AddApplicationInsights(configuration =>
            {
                configuration.ConnectionString = settings.ApplicationInsightsConnectionString;
            }, _ => { });
        }

        builder.Logging.SetMinimumLevel(settings.LogLevel);

        return builder;
    }
}

