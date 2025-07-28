using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using System.Linq;

namespace Dotnet.AzureDevOps.Overview.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzureDevOpsOverviewIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WikiClient _wikiClient;
        private readonly DashboardClient _dashboardClient;
        private readonly SummaryClient _summaryClient;
        private readonly List<Guid> _createdWikis = [];

        public DotnetAzureDevOpsOverviewIntegrationTests()
        {
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();

            _wikiClient = new WikiClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
            _dashboardClient = new DashboardClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
            _summaryClient = new SummaryClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        [Fact]
        public async Task CreateReadDeleteWiki_SucceedsAsync()
        {
            var create = new WikiCreateOptions
            {
                Name = $"it-wiki-{UtcStamp()}",
                ProjectId = Guid.Parse(_azureDevOpsConfiguration.ProjectId),
                Type = WikiType.CodeWiki,
                RepositoryId = Guid.Parse(_azureDevOpsConfiguration.RepositoryId),
                Version = new GitVersionDescriptor
                {
                    VersionType = GitVersionType.Branch,
                    Version = _azureDevOpsConfiguration.MainBranchName
                },
                MappedPath = "/"

            };

            Guid id = await _wikiClient.CreateWikiAsync(create);
            _createdWikis.Add(id);

            WikiV2? wiki = null;
            wiki = await _wikiClient.GetWikiAsync(id);

            Assert.NotNull(wiki);
            Assert.Equal(create.Name, wiki!.Name);

            IReadOnlyList<WikiV2> list = await _wikiClient.ListWikisAsync();
            Assert.Contains(list, w => w.Id == id);

            await _wikiClient.DeleteWikiAsync(id);
            _createdWikis.Remove(id);

            WikiV2? afterDelete = await _wikiClient.GetWikiAsync(id);
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task CreateUpdateReadDeletePage_SucceedsAsync()
        {
            // create wiki to host page
            var create = new WikiCreateOptions
            {
                Name = $"it-wiki-{UtcStamp()}",
                ProjectId = Guid.Parse(_azureDevOpsConfiguration.ProjectId),
                Type = WikiType.CodeWiki,
                RepositoryId = Guid.Parse(_azureDevOpsConfiguration.RepositoryId),
                Version = new GitVersionDescriptor
                {
                    VersionType = GitVersionType.Branch,
                    Version = _azureDevOpsConfiguration.MainBranchName
                },
                MappedPath = "/"

            };

            Guid id = await _wikiClient.CreateWikiAsync(create);
            _createdWikis.Add(id);

            const string path = "/Home.md";
            WikiPageResponse? response = null;

            try
            {
                response = await _wikiClient.GetPageAsync(id, path);
            }
            catch(HttpRequestException)
            {
                // Page doesn't exist — that's fine, we’ll create it.
            }

            var wikiPageUpdateOptions = new WikiPageUpdateOptions
            {
                Path = path,
                Content = "# Hello world",
                Version = response?.ETag?.FirstOrDefault() ?? string.Empty
            };

            var gitVersionDescriptor = new GitVersionDescriptor
            {
                VersionType = GitVersionType.Branch,
                Version = _azureDevOpsConfiguration.MainBranchName
            };

            int? pageId = await _wikiClient.CreateOrUpdatePageAsync(id, wikiPageUpdateOptions, gitVersionDescriptor);
            Assert.True(pageId.HasValue);

            // update page
            // Ensure we get the *fresh* version of the page
            WikiPageResponse? getResponse = await _wikiClient.GetPageAsync(id, path);
            string? etag = getResponse?.ETag?.FirstOrDefault();

            if(string.IsNullOrEmpty(etag))
            {
                throw new InvalidOperationException("ETag required for update, but not found.");
            }

            await _wikiClient.CreateOrUpdatePageAsync(id, new WikiPageUpdateOptions
            {
                Path = path,
                Content = "# Updated",
                Version = etag
            }, gitVersionDescriptor);

            // Optional: Confirm update
            WikiPageResponse? page = await _wikiClient.GetPageAsync(id, path);
            Assert.NotNull(page);
            Assert.Contains("Updated", page!.Page.Content); // confirm update

            await _wikiClient.DeletePageAsync(id, path, gitVersionDescriptor);
        }

        [Fact]
        public async Task ListDashboards_ReturnsCollectionAsync()
        {
            IReadOnlyList<Dashboard> dashboards = await _dashboardClient.ListDashboardsAsync();
            Assert.NotNull(dashboards);
        }

        [Fact]
        public async Task GetDashboard_ReturnsDashboardAsync()
        {
            IReadOnlyList<Dashboard> dashboards = await _dashboardClient.ListDashboardsAsync();
            Dashboard? first = dashboards.FirstOrDefault();
            if (first == null)
                return;

            Dashboard? dashboard = await _dashboardClient.GetDashboardAsync(first.Id);
            Assert.NotNull(dashboard);
            Assert.Equal(first.Id, dashboard!.Id);
        }

        [Fact]
        public async Task GetProjectSummary_ReturnsProjectAsync()
        {
            TeamProject? project = await _summaryClient.GetProjectSummaryAsync();
            Assert.NotNull(project);
            Assert.Equal(_azureDevOpsConfiguration.ProjectName, project!.Name);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(Guid id in _createdWikis.AsEnumerable().Reverse())
            {
                await _wikiClient.DeleteWikiAsync(id);
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
