using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

public sealed record McpServerSettings
{
    public LogLevel LogLevel { get; init; } = LogLevel.Trace;

    public bool EnableOpenTelemetry { get; init; } = true;

    public int Port { get; init; } = 5050;
}
