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
using System.Net.Http.Headers;
using System.Text;

namespace Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;

public static class CoreClientsServiceCollectionExtensions
{
    public static IServiceCollection AddAzureDevOpsClients(this IServiceCollection services)
    {
        // Configure HttpClient for Azure DevOps API calls
        services.AddHttpClient<IWorkItemsClient, WorkItemsClient>((provider, client) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            client.BaseAddress = new Uri(config.OrganizationUrl);
            string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        })
        .AddTypedClient<IWorkItemsClient>((httpClient, provider) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            ILogger<WorkItemsClient>? logger = provider.GetService<ILogger<WorkItemsClient>>();
            return new WorkItemsClient(httpClient, config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddHttpClient<IReposClient, ReposClient>((provider, client) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            client.BaseAddress = new Uri(config.OrganizationUrl);
            string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        })
        .AddTypedClient<IReposClient>((httpClient, provider) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            ILogger<ReposClient>? logger = provider.GetService<ILogger<ReposClient>>();
            return new ReposClient(httpClient, config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        services.AddHttpClient<IArtifactsClient, ArtifactsClient>((provider, client) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            string orgUrl = config.OrganizationUrl.Replace("https://dev.azure.com", "https://feeds.dev.azure.com");
            client.BaseAddress = new Uri(orgUrl);
            string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        })
        .AddTypedClient<IArtifactsClient>((httpClient, provider) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            ILogger<ArtifactsClient>? logger = provider.GetService<ILogger<ArtifactsClient>>();
            return new ArtifactsClient(httpClient, config.ProjectName, logger);
        });

        services.AddHttpClient<ISearchClient, SearchClient>((provider, client) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            client.BaseAddress = new Uri(config.SearchOrganizationUrl);
            string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddTypedClient<ISearchClient>((httpClient, provider) =>
        {
            ILogger<SearchClient>? logger = provider.GetService<ILogger<SearchClient>>();
            return new SearchClient(httpClient, logger);
        });

        services.AddHttpClient<IProjectSettingsClient, ProjectSettingsClient>((provider, client) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            client.BaseAddress = new Uri(config.OrganizationUrl);
            string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        })
        .AddTypedClient<IProjectSettingsClient>((httpClient, provider) =>
        {
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            ILogger<ProjectSettingsClient>? logger = provider.GetService<ILogger<ProjectSettingsClient>>();
            return new ProjectSettingsClient(httpClient, config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
        });

        // Register clients that only use VssConnection (no direct HttpClient)
        services.AddScoped<IPipelinesClient>(provider =>
        {
            ILogger<PipelinesClient>? logger = provider.GetService<ILogger<PipelinesClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new PipelinesClient(config.OrganizationUrl, config.ProjectName, config.PersonalAccessToken, logger);
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

        services.AddScoped<IIdentityClient>(provider =>
        {
            ILogger<IdentityClient>? logger = provider.GetService<ILogger<IdentityClient>>();
            AzureDevOpsConfiguration config = provider.GetRequiredService<AzureDevOpsConfiguration>();
            return new IdentityClient(config.OrganizationUrl, config.PersonalAccessToken, logger);
        });

        return services;
    }
}
