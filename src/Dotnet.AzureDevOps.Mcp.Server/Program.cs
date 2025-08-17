using Azure.Monitor.OpenTelemetry.Exporter;
using Dotnet.AzureDevOps.Mcp.Server;
using Dotnet.AzureDevOps.Mcp.Server.McpServer;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Dotnet.AzureDevOps.Mcp.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.ConfigureSettings();
        builder.ConfigureLogging();

        McpServerSettings mcpSettings = builder.Configuration.GetSection("McpServer").Get<McpServerSettings>() ?? new();

        // Add Application Insights if enabled
        if (mcpSettings.EnableApplicationInsights)
        {
            builder.Services.AddApplicationInsightsTelemetry();
        }

        if (mcpSettings.EnableOpenTelemetry)
        {
            IOpenTelemetryBuilder openTelemetryBuilder = builder.Services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService(serviceName: "azure-devops-mcp-server"))
                .WithTracing(t => t
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Azure.*"))
                .WithMetrics(m => m
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation());

            // Add Azure Monitor exporters if Application Insights is enabled
            if (mcpSettings.EnableApplicationInsights)
            {
                openTelemetryBuilder
                    .WithTracing(t => t.AddAzureMonitorTraceExporter())
                    .WithMetrics(m => m.AddAzureMonitorMetricExporter());
            }
        }

        builder.ConfigureMcpServer();
        builder.Services.AddMcpHealthChecks();

        WebApplication app = builder.Build();

        app.MapMcp("/mcp");
        app.MapMcpHealthEndpoint();

        await app.RunAsync();
    }
}