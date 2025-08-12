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

        services.Configure<AzureDevOpsConfiguration>(options =>
        ConfigureAzureDevOpsOptions(builder.Configuration, options));

        services.AddAzureDevOpsClients();

        IMcpServerBuilder mcpServerBuilder = services.AddMcpServer();

        ConfigureMcpServerTools(mcpServerBuilder);

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

    private static void ConfigureAzureDevOpsOptions(IConfiguration configuration, AzureDevOpsConfiguration options)
    {
        ValidateAndSetOrganizationUrl(configuration, options);
        ValidateAndSetSearchOrganizationUrl(configuration, options);
        ValidateAndSetProjectName(configuration, options);
        ValidateAndSetPersonalAccessToken(configuration, options);
    }

    private static void ValidateAndSetOrganizationUrl(IConfiguration configuration, AzureDevOpsConfiguration options)
    {
        string? orgUrl = configuration["AZURE_DEVOPS_ORGANIZATION_URL"];
        if(string.IsNullOrWhiteSpace(orgUrl))
            throw new InvalidOperationException("AZURE_DEVOPS_ORGANIZATION_URL environment variable is required and cannot be null or empty.");
        if(!Uri.TryCreate(orgUrl, UriKind.Absolute, out Uri? orgUri) || orgUri.Scheme != "https")
            throw new InvalidOperationException("AZURE_DEVOPS_ORGANIZATION_URL must be a valid HTTPS URL.");
        options.OrganizationUrl = orgUrl;
    }

    private static void ValidateAndSetSearchOrganizationUrl(IConfiguration configuration, AzureDevOpsConfiguration options)
    {
        string? searchOrgUrl = configuration["AZURE_DEVOPS_SEARCH_ORGANIZATION_URL"];
        if(string.IsNullOrWhiteSpace(searchOrgUrl))
            throw new InvalidOperationException("AZURE_DEVOPS_SEARCH_ORGANIZATION_URL environment variable is required and cannot be null or empty.");
        if(!Uri.TryCreate(searchOrgUrl, UriKind.Absolute, out Uri? searchUri) || searchUri.Scheme != "https")
            throw new InvalidOperationException("AZURE_DEVOPS_SEARCH_ORGANIZATION_URL must be a valid HTTPS URL.");
        options.SearchOrganizationUrl = searchOrgUrl;
    }

    private static void ValidateAndSetProjectName(IConfiguration configuration, AzureDevOpsConfiguration options)
    {
        string? projectName = configuration["AZURE_DEVOPS_PROJECT_NAME"];
        if(string.IsNullOrWhiteSpace(projectName))
            throw new InvalidOperationException("AZURE_DEVOPS_PROJECT_NAME environment variable is required and cannot be null or empty.");
        options.ProjectName = projectName;
    }

    private static void ValidateAndSetPersonalAccessToken(IConfiguration configuration, AzureDevOpsConfiguration options)
    {
        string? pat = configuration["AZURE_DEVOPS_PAT"];
        if(string.IsNullOrWhiteSpace(pat))
            throw new InvalidOperationException("AZURE_DEVOPS_PAT environment variable is required and cannot be null or empty.");
        if(pat.Length < 20) // Basic validation for PAT format
            throw new InvalidOperationException("AZURE_DEVOPS_PAT appears to be too short to be a valid Personal Access Token.");
        options.PersonalAccessToken = pat;
    }

    private static void ConfigureMcpServerTools(IMcpServerBuilder mcpServerBuilder) => mcpServerBuilder.WithHttpTransport()
            .WithTools<EchoTool>()
            .WithTools<BoardsTools>()
            .WithTools<ArtifactsTools>()
            .WithTools<OverviewTools>()
            .WithTools<PipelinesTools>()
            .WithTools<ProjectSettingsTools>()
            .WithTools<ReposTools>()
            .WithTools<SearchTools>()
            .WithTools<TestPlansTools>();
}

