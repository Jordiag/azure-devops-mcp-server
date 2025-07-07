using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

internal static class HostingExtensionsSettings
{
    public static WebApplicationBuilder ConfigureSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration
               .AddEnvironmentVariables("MCP_")
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return builder;
    }
}