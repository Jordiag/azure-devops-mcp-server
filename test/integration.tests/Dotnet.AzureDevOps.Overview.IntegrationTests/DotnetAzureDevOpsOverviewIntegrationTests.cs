using System.Threading;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Dotnet.AzureDevOps.Overview.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Overview)]
    public class DotnetAzureDevOpsOverviewIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WikiClient _wikiClient;
        private readonly List<Guid> _createdWikis = [];
        private readonly ProjectSettingsClient _projectSettingsClient;

        public DotnetAzureDevOpsOverviewIntegrationTests(IntegrationTestFixture fixture)
        {
            _azureDevOpsConfiguration = fixture.Configuration;
            _wikiClient = fixture.WikiClient;
            _projectSettingsClient = fixture.ProjectSettingsClient;
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
            WikiV2? afterDelete = null;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                afterDelete = await _wikiClient.GetWikiAsync(id);
                return afterDelete is null;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
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
        public async Task DashboardSummaryAndWikiHelpers_SucceedAsync()
        {
            DashboardClient dashboardClient = new DashboardClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);

            IReadOnlyList<Dashboard> dashboards = await dashboardClient.ListDashboardsAsync();
            Assert.NotEmpty(dashboards);
            string teamName = "Dotnet.McpIntegrationTest Team";
            List<WebApiTeam> teams = await _projectSettingsClient.GetAllTeamsAsync();
            WebApiTeam? team = teams.FirstOrDefault(t => t.Name == teamName);
            Dashboard? dashboard = dashboards.FirstOrDefault(d => d.OwnerId == team?.Id) ?? dashboards[0];
            Guid dashboardId = dashboard?.Id ?? Guid.Empty;
            if (dashboardId == Guid.Empty)
            {
                throw new InvalidOperationException("No dashboard found for the specified team or project.");
            }

            dashboard = await dashboardClient.GetDashboardAsync(dashboardId, teamName);
            Assert.NotNull(dashboard);

            var summaryClient = new SummaryClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);

            TeamProject? projectSummary = await summaryClient.GetProjectSummaryAsync();
            Assert.NotNull(projectSummary);

            IReadOnlyList<WikiV2> wikis = await _wikiClient.ListWikisAsync();
            Guid wikiId;
            string wikiname;
            if(wikis.Count == 0)
            {
                var wikiOptions = new WikiCreateOptions
                {
                    Name = $"pages-wiki-{UtcStamp()}",
                    ProjectId = Guid.Parse(_azureDevOpsConfiguration.ProjectId),
                    Type = WikiType.CodeWiki,
                    RepositoryId = Guid.Parse(_azureDevOpsConfiguration.RepositoryId),
                    Version = new GitVersionDescriptor
                    {
                        VersionType = GitVersionType.Branch,
                        Version = _azureDevOpsConfiguration.MainBranchName
                    },
                    MappedPath = $"/",
                };
                wikiname = wikiOptions.Name;
                wikiId = await _wikiClient.CreateWikiAsync(wikiOptions);
            }
            else
            {
                wikiname = wikis[0].Name;
                wikiId = wikis[0].Id;
            }
            _createdWikis.Add(wikiId);

            string wikiPath = $"/Home-{UtcStamp()}.md";
            var createPage = new WikiPageUpdateOptions
            {
                Path = wikiPath,
                Content = "# Searchable",
                Version = string.Empty
            };

            var versionDescriptor = new GitVersionDescriptor
            {
                VersionType = GitVersionType.Branch,
                Version = _azureDevOpsConfiguration.MainBranchName
            };

            await _wikiClient.CreateOrUpdatePageAsync(wikiId, createPage, versionDescriptor);

            IReadOnlyList<WikiPageDetail> pages = await _wikiClient.ListPagesAsync(
                wikiId,
                new WikiPagesBatchOptions { Top = 100 },
                null);
            Assert.Contains(pages, p => p.Path == wikiPath);

            string? text = await _wikiClient.GetPageTextAsync(wikiId, wikiPath);
            Assert.Contains("Searchable", text);

            var searchClient = new SearchClient(
                _azureDevOpsConfiguration.Organisation,
                _azureDevOpsConfiguration.PersonalAccessToken);

            var searchOptions = new Dotnet.AzureDevOps.Core.Search.Options.WikiSearchOptions
            {
                SearchText = "Searchable",
                Project = [_azureDevOpsConfiguration.ProjectName],
                Wiki = [wikiname],
                IncludeFacets = false,
                Skip = 0,
                Top = 1
            };

            AzureDevOpsActionResult<string> result =  await searchClient.SearchWikiAsync(searchOptions);
            Assert.False(string.IsNullOrEmpty(result.Value));
            Assert.True(result.Value.Length > 0, "Expected result to contain at least one item, but it was empty.");

            WikiPageResponse wikiPageResponse = await _wikiClient.DeletePageAsync(wikiId, wikiPath, versionDescriptor);

            Assert.True(wikiPageResponse != null, "Expected wiki page response to be non-null after deletion.");
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
            DateTime.UtcNow.ToString("O").Replace(':', '-');
    }
}
