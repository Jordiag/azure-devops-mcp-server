using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.TestPlans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;

public static class CoreClientsServiceCollectionExtensions
{
    public static IServiceCollection AddAzureDevOpsClients(this IServiceCollection services)
    {
        // Register all Core clients as scoped services
        services.AddScoped<IWorkItemsClient>(provider =>
        {
            ILogger<WorkItemsClient>? logger = provider.GetService<ILogger<WorkItemsClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new WorkItemsClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IReposClient>(provider =>
        {
            ILogger<ReposClient>? logger = provider.GetService<ILogger<ReposClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ReposClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IPipelinesClient>(provider =>
        {
            ILogger<PipelinesClient>? logger = provider.GetService<ILogger<PipelinesClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new PipelinesClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IArtifactsClient>(provider =>
        {
            ILogger<ArtifactsClient>? logger = provider.GetService<ILogger<ArtifactsClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ArtifactsClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<ITestPlansClient>(provider =>
        {
            ILogger<TestPlansClient>? logger = provider.GetService<ILogger<TestPlansClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new TestPlansClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IWikiClient>(provider =>
        {
            ILogger<WikiClient>? logger = provider.GetService<ILogger<WikiClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new WikiClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IDashboardClient>(provider =>
        {
            ILogger<DashboardClient>? logger = provider.GetService<ILogger<DashboardClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new DashboardClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<ISummaryClient>(provider =>
        {
            ILogger<SummaryClient>? logger = provider.GetService<ILogger<SummaryClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new SummaryClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddScoped<ISearchClient>(provider =>
        {
            ILogger<SearchClient>? logger = provider.GetService<ILogger<SearchClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new SearchClient(config.OrganizationUrl, config.PersonalAccessToken, logger);
        });

        services.AddScoped<IProjectSettingsClient>(provider =>
        {
            ILogger<ProjectSettingsClient>? logger = provider.GetService<ILogger<ProjectSettingsClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ProjectSettingsClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        return services;
    }
}

public class AzureDevOpsConfiguration
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty; 
    public string PersonalAccessToken { get; set; } = string.Empty;
}
