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
        builder.Logging.SetMinimumLevel(settings.LogLevel);

        return builder;
    }
}
