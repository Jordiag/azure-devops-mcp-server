using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.TestPlans;
using Xunit;

namespace Dotnet.AzureDevOps.Tests.Common;

public class IntegrationTestFixture : IAsyncLifetime
{
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

    public Task InitializeAsync()
    {
        Configuration = AzureDevOpsConfiguration.FromEnvironment();

        WorkItemsClient = new WorkItemsClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        ReposClient = new ReposClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        ProjectSettingsClient = new ProjectSettingsClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        PipelinesClient = new PipelinesClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        ArtifactsClient = new ArtifactsClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        WikiClient = new WikiClient(
            Configuration.OrganisationUrl,
            Configuration.ProjectName,
            Configuration.PersonalAccessToken);

        SearchClient = new SearchClient(
            Configuration.SearchOrganisationUrl,
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

    public Task DisposeAsync() => Task.CompletedTask;
}

