using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

public sealed record McpServerSettings
{
    public LogLevel LogLevel { get; init; } = LogLevel.Trace;

    public bool EnableOpenTelemetry { get; init; } = true;

    public bool EnableApplicationInsights { get; init; } = false;

    public int Port { get; init; } = 5050;

    public string? Environment { get; init; } = null;
}
