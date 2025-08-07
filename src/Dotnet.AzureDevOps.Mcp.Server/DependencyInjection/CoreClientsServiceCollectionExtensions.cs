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
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new WorkItemsClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IReposClient>(provider =>
        {
            ILogger<ReposClient>? logger = provider.GetService<ILogger<ReposClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ReposClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IPipelinesClient>(provider =>
        {
            ILogger<PipelinesClient>? logger = provider.GetService<ILogger<PipelinesClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new PipelinesClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IArtifactsClient>(provider =>
        {
            ILogger<ArtifactsClient>? logger = provider.GetService<ILogger<ArtifactsClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ArtifactsClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<ITestPlansClient>(provider =>
        {
            ILogger<TestPlansClient>? logger = provider.GetService<ILogger<TestPlansClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new TestPlansClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IWikiClient>(provider =>
        {
            ILogger<WikiClient>? logger = provider.GetService<ILogger<WikiClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new WikiClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IDashboardClient>(provider =>
        {
            ILogger<DashboardClient>? logger = provider.GetService<ILogger<DashboardClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new DashboardClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<ISummaryClient>(provider =>
        {
            ILogger<SummaryClient>? logger = provider.GetService<ILogger<SummaryClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new SummaryClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<ISearchClient>(provider =>
        {
            ILogger<SearchClient>? logger = provider.GetService<ILogger<SearchClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new SearchClient(configuration.SearchOrganizationUrl, configuration.PersonalAccessToken, logger);
        });

        services.AddScoped<IProjectSettingsClient>(provider =>
        {
            ILogger<ProjectSettingsClient>? logger = provider.GetService<ILogger<ProjectSettingsClient>>();
            AzureDevOpsConfiguration configuration = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new ProjectSettingsClient(configuration.OrganizationUrl, configuration.ProjectName, configuration.PersonalAccessToken, logger);
        });

        return services;
    }
}
