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
        private readonly IOverviewClient _overviewClient;
        private readonly List<Guid> _createdWikis = [];
        private readonly ProjectSettingsClient _projectSettingsClient;
        private readonly IntegrationTestFixture _fixture;

        public DotnetAzureDevOpsOverviewIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _azureDevOpsConfiguration = fixture.Configuration;
            _overviewClient = fixture.OverviewClient;
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

            Guid id = Guid.Empty;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<Guid> createResult = await _overviewClient.CreateWikiAsync(create);
                if(!createResult.IsSuccessful)
                    return false;
                id = createResult.Value;
                return id != Guid.Empty;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            if(id == Guid.Empty)
            {
                throw new InvalidOperationException("Wiki creation failed, returned ID is empty.");
            }
            _createdWikis.Add(id);

            AzureDevOpsActionResult<WikiV2> wikiResult = await _overviewClient.GetWikiAsync(id);
            WikiV2? wiki = wikiResult.Value;

            Assert.NotNull(wiki);
            Assert.Equal(create.Name, wiki!.Name);

            AzureDevOpsActionResult<IReadOnlyList<WikiV2>> listResult = await _overviewClient.ListWikisAsync();
            IReadOnlyList<WikiV2> list = listResult.Value;
            Assert.Contains(list, w => w.Id == id);

            _ = await _overviewClient.DeleteWikiAsync(id);
            _createdWikis.Remove(id);
            
            WikiV2? afterDelete = null;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<WikiV2> afterDeleteResult = await _overviewClient.GetWikiAsync(id);
                afterDelete = afterDeleteResult.Value;
                return !afterDeleteResult.IsSuccessful || afterDelete is null;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
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

            Guid id = Guid.Empty;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<Guid> createResult = await _overviewClient.CreateWikiAsync(create);
                if(!createResult.IsSuccessful)
                    return false;
                id = createResult.Value;
                return true;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            _createdWikis.Add(id);

            const string path = "/Home.md";
            AzureDevOpsActionResult<WikiPageResponse> responseResult = await _overviewClient.GetPageAsync(id, path);
            WikiPageResponse? response = responseResult.Value;

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

            AzureDevOpsActionResult<int> pageIdResult = await _overviewClient.CreateOrUpdatePageAsync(id, wikiPageUpdateOptions, gitVersionDescriptor);
            Assert.True(pageIdResult.IsSuccessful);
            int? pageId = pageIdResult.IsSuccessful ? pageIdResult.Value : (int?)null;
            Assert.True(pageId.HasValue);

            // update page
            // Ensure we get the *fresh* version of the page
            AzureDevOpsActionResult<WikiPageResponse> getResponseResult = await _overviewClient.GetPageAsync(id, path);
            WikiPageResponse? getResponse = getResponseResult.Value;
            string? etag = getResponse?.ETag?.FirstOrDefault();

            if(string.IsNullOrEmpty(etag))
            {
                throw new InvalidOperationException("ETag required for update, but not found.");
            }

            _ = await _overviewClient.CreateOrUpdatePageAsync(id, new WikiPageUpdateOptions
            {
                Path = path,
                Content = "# Updated",
                Version = etag
            }, gitVersionDescriptor);

            // Optional: Confirm update
            AzureDevOpsActionResult<WikiPageResponse> pageResult = await _overviewClient.GetPageAsync(id, path);
            WikiPageResponse? page = pageResult.Value;
            Assert.NotNull(page);
            Assert.Contains("Updated", page!.Page.Content); // confirm update

            AzureDevOpsActionResult<WikiPageResponse> deletePageResult = await _overviewClient.DeletePageAsync(id, path, gitVersionDescriptor);
        }

        [Fact]
        public async Task DashboardSummaryAndWikiHelpers_SucceedAsync()
        {
            AzureDevOpsActionResult<IReadOnlyList<Dashboard>> dashboardsResult = await _overviewClient.ListDashboardsAsync();
            IReadOnlyList<Dashboard> dashboards = dashboardsResult.Value;
            Assert.NotEmpty(dashboards);
            string teamName = "Dotnet.McpIntegrationTest Team";
            AzureDevOpsActionResult<List<WebApiTeam>> teamsResult = await _projectSettingsClient.GetAllTeamsAsync();
            List<WebApiTeam> teams = teamsResult.Value ?? new List<WebApiTeam>();
            WebApiTeam? team = teams.FirstOrDefault(t => t.Name == teamName);
            Dashboard? dashboard = dashboards.FirstOrDefault(d => d.OwnerId == team?.Id) ?? dashboards[0];
            Guid dashboardId = dashboard?.Id ?? Guid.Empty;
            if(dashboardId == Guid.Empty)
            {
                throw new InvalidOperationException("No dashboard found for the specified team or project.");
            }

            AzureDevOpsActionResult<Dashboard> dashboardResult = await _overviewClient.GetDashboardAsync(dashboardId, teamName);
            dashboard = dashboardResult.Value;
            Assert.NotNull(dashboard);

            AzureDevOpsActionResult<TeamProject> projectSummaryResult = await _overviewClient.GetProjectSummaryAsync();
            TeamProject? projectSummary = projectSummaryResult.Value;
            Assert.NotNull(projectSummary);

            AzureDevOpsActionResult<IReadOnlyList<WikiV2>> wikisResult = await _overviewClient.ListWikisAsync();
            IReadOnlyList<WikiV2> wikis = wikisResult.Value;
            Guid wikiId = Guid.Empty;
            string wikiName;
            if(wikis.Count == 0)
            {
                var wikiOptions = new WikiCreateOptions
                {
                    Name = $"page-wiki-{UtcStamp()}",
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
                wikiName = wikiOptions.Name;

                await WaitHelper.WaitUntilAsync(async () =>
                {
                    try
                    {
                        AzureDevOpsActionResult<Guid> createWikiResult = await _overviewClient.CreateWikiAsync(wikiOptions);
                        if(!createWikiResult.IsSuccessful)
                            return false;
                        wikiId = createWikiResult.Value;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
            }
            else
            {
                wikiName = wikis[0].Name;
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

            AzureDevOpsActionResult<int> pageIdActionResult = await _overviewClient.CreateOrUpdatePageAsync(wikiId, createPage, versionDescriptor);
            Assert.True(pageIdActionResult.IsSuccessful, "Expected page creation to be successful.");

            AzureDevOpsActionResult<WikiPageResponse>? pageResult = null;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                pageResult = await _overviewClient.GetPageAsync(wikiId, wikiPath);
                return pageResult.IsSuccessful && pageResult.Value?.Page?.Path == wikiPath;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            string needlessText = "Searchable";
            AzureDevOpsActionResult<string>? textResult = null;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                textResult = await _overviewClient.GetPageTextAsync(wikiId, wikiPath);
                return textResult.IsSuccessful && textResult.Value?.Contains(needlessText) == true;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            string text = textResult!.Value!;
            Assert.Contains(needlessText, text);

            HttpClient searchHttpClient = _fixture.CreateHttpClient(_azureDevOpsConfiguration.SearchOrganisationUrl);
            searchHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            SearchClient searchClient = new(searchHttpClient);

            var searchOptions = new Dotnet.AzureDevOps.Core.Search.Options.WikiSearchOptions
            {
                SearchText = "Searchable",
                Project = [_azureDevOpsConfiguration.ProjectName],
                Wiki = [wikiName],
                IncludeFacets = false,
                Skip = 0,
                Top = 1
            };

            AzureDevOpsActionResult<string> result = await searchClient.SearchWikiAsync(searchOptions);
            Assert.False(string.IsNullOrEmpty(result.Value));
            Assert.True(result.Value.Length > 0, "Expected result to contain at least one item, but it was empty.");

            AzureDevOpsActionResult<WikiPageResponse> wikiPageResponseResult = await _overviewClient.DeletePageAsync(wikiId, wikiPath, versionDescriptor);

            Assert.True(wikiPageResponseResult.Value != null, "Expected wiki page response to be non-null after deletion.");
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(Guid id in _createdWikis.AsEnumerable().Reverse())
            {
                _ = await _overviewClient.DeleteWikiAsync(id);
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("O").Replace(':', '-');
    }
}
