using System.Net.Http.Headers;
using System.Text;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.TestPlans;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dotnet.AzureDevOps.Tests.Common;

public class IntegrationTestFixture : IAsyncLifetime
{
    private IServiceProvider? _serviceProvider;
    
    public AzureDevOpsConfiguration Configuration { get; private set; } = null!;
    public WorkItemsClient WorkItemsClient { get; private set; } = null!;
    public ReposClient ReposClient { get; private set; } = null!;
    public ProjectSettingsClient ProjectSettingsClient { get; private set; } = null!;
    public PipelinesClient PipelinesClient { get; private set; } = null!;
    public ArtifactsClient ArtifactsClient { get; private set; } = null!;
    public WikiClient WikiClient { get; private set; } = null!;
    public SearchClient SearchClient { get; private set; } = null!;
    public IdentityClient IdentityClient { get; private set; } = null!;
    public TestPlansClient TestPlansClient { get; private set; } = null!;
    
    /// <summary>
    /// Gets an HttpClient configured for the specified base URL with proper authentication.
    /// Use this method in integration tests instead of creating HttpClient instances directly.
    /// </summary>
    public HttpClient CreateHttpClient(string baseUrl)
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized. Call InitializeAsync first.");
            
        IHttpClientFactory factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        HttpClient client = factory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        ConfigureAuthentication(client, Configuration.PersonalAccessToken);
        return client;
    }

    public Task InitializeAsync()
    {
        Configuration = AzureDevOpsConfiguration.FromEnvironment();

        // Configure services with HttpClientFactory
        ServiceCollection services = new();
        
        // Add HttpClient factory - provides proper connection pooling and resource management
        services.AddHttpClient();
        
        // Build service provider
        _serviceProvider = services.BuildServiceProvider();
        
        // Create clients using HttpClientFactory pattern for HTTP-based clients
        HttpClient workItemsHttpClient = CreateHttpClient(Configuration.OrganisationUrl);
        WorkItemsClient = new WorkItemsClient(
            workItemsHttpClient,
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        HttpClient reposHttpClient = CreateHttpClient(Configuration.OrganisationUrl);
        ReposClient = new ReposClient(
            reposHttpClient,
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        HttpClient projectSettingsHttpClient = CreateHttpClient(Configuration.OrganisationUrl);
        ProjectSettingsClient = new ProjectSettingsClient(
            projectSettingsHttpClient,
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        HttpClient artifactsHttpClient = CreateHttpClient(Configuration.OrganisationUrl.Replace("https://dev.azure.com", "https://feeds.dev.azure.com"));
        ArtifactsClient = new ArtifactsClient(artifactsHttpClient, Configuration.ProjectName);

        HttpClient searchHttpClient = CreateHttpClient(Configuration.SearchOrganisationUrl);
        searchHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        SearchClient = new SearchClient(searchHttpClient);

        // Clients that don't use HttpClient (VssConnection-based) - no changes needed
        PipelinesClient = new PipelinesClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);
            
        WikiClient = new WikiClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);
            
        IdentityClient = new IdentityClient(
            Configuration.OrganisationUrl,
            Configuration.PersonalAccessToken);
            
        TestPlansClient = new TestPlansClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        return Task.CompletedTask;
    }

    private static void ConfigureAuthentication(HttpClient client, string personalAccessToken)
    {
        string token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        client.DefaultRequestHeaders.Add("User-Agent", "azure-devops-mcp-server");
    }

    public Task DisposeAsync()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
        return Task.CompletedTask;
    }
}

