using Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;
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

        // Register Azure DevOps configuration
        services.Configure<AzureDevOpsConfiguration>(options =>
        {
            options.OrganizationUrl = builder.Configuration["AZURE_DEVOPS_ORGANIZATION_URL"] ?? "";
            options.ProjectName = builder.Configuration["AZURE_DEVOPS_PROJECT_NAME"] ?? "";
            options.PersonalAccessToken = builder.Configuration["AZURE_DEVOPS_PAT"] ?? "";
        });

        // Register Azure DevOps clients
        services.AddAzureDevOpsClients();

        IMcpServerBuilder mcpServerBuilder = services.AddMcpServer();

        mcpServerBuilder.WithHttpTransport()
            .WithTools<EchoTool>()
            .WithTools<BoardsTools>()
            .WithTools<ArtifactsTools>()
            .WithTools<OverviewTools>()
            .WithTools<PipelinesTools>()
            .WithTools<ProjectSettingsTools>()
            .WithTools<ReposTools>()
            .WithTools<SearchTools>()
            .WithTools<TestPlansTools>();

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

