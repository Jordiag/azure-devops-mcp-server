using Dotnet.AzureDevOps.Mcp.Server.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.AzureDevOps.Mcp.Server.McpServer;

internal static class HostingExtensionsMcpServer
{
    public static WebApplicationBuilder ConfigureMcpServer(this WebApplicationBuilder builder)
    {
        McpServerSettings settings = builder.Configuration
                                            .GetSection("McpServer")
                                            .Get<McpServerSettings>() ?? new();

        IServiceCollection services = builder.Services;
        IMcpServerBuilder mcp = services.AddMcpServer();

        mcp.WithHttpTransport()
            .WithTools<EchoTool>()
            .WithTools<BoardsTools>();

        if(settings.EnableOpenTelemetry)
            services.AddOpenTelemetry()
                    .WithMetrics()
                    .WithTracing();

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.ListenAnyIP(settings.Port);
        });

        return builder;
    }
}

