using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

internal static class HostingExtensionsLogging
{
    private const string ServiceName = "azure-devops-mcp-server";

    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        McpServerSettings settings = builder.Configuration
                                            .GetSection("McpServer")
                                            .Get<McpServerSettings>() ?? new();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        if(settings.EnableApplicationInsights)
        {
            builder.Logging.AddOpenTelemetry(ot =>
            {
                ot.IncludeFormattedMessage = true;
                ot.IncludeScopes = false;
                ot.ParseStateValues = true;
                ot.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName));
                ot.AddAzureMonitorLogExporter(); // Connection string will be automatically detected
            });
        }

        builder.Logging.SetMinimumLevel(settings.LogLevel);

        return builder;
    }
}

