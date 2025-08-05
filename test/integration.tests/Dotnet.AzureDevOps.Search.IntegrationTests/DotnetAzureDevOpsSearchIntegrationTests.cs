using System.Net;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.Search.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Dotnet.AzureDevOps.Search.IntegrationTests;

[TestType(TestType.Integration)]
[Component(Component.Search)]
public class DotnetAzureDevOpsSearchIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
    private readonly WikiClient _wikiClient;
    private readonly WorkItemsClient _workItemsClient;
    private readonly SearchClient _searchClient;
    private readonly List<Guid> _createdWikis = [];
    private readonly List<int> _createdWorkItemIds = [];

    public DotnetAzureDevOpsSearchIntegrationTests(IntegrationTestFixture fixture)
    {
        _azureDevOpsConfiguration = fixture.Configuration;
        _wikiClient = fixture.WikiClient;
        _workItemsClient = fixture.WorkItemsClient;
        _searchClient = fixture.SearchClient;
    }

    [Fact]
    public async Task WikiSearch_ReturnsResultsAsync()
    {
        var wikiCreateOptions = new WikiCreateOptions
        {
            Name = $"it-wiki-{UtcStamp()}",
            ProjectId = Guid.Parse(_azureDevOpsConfiguration.ProjectId),
            RepositoryId = Guid.Parse(_azureDevOpsConfiguration.RepositoryId),
            Type = WikiType.CodeWiki,
            MappedPath = "/",
            Version = new GitVersionDescriptor
            {
                VersionType = GitVersionType.Branch,
                Version = _azureDevOpsConfiguration.MainBranchName
            }
        };
        await WaitHelper.WaitUntilAsync(async () =>
        {
            try
            {
                await _wikiClient.ListWikisAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
        Guid wikiId = await _wikiClient.CreateWikiAsync(wikiCreateOptions);
        _createdWikis.Add(wikiId);

        string wikiPath = $"/Home-{UtcStamp()}.md";
        var pageOptions = new WikiPageUpdateOptions
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

        await WaitHelper.WaitUntilAsync(async () =>
        {
            try
            {
                await _wikiClient.CreateOrUpdatePageAsync(wikiId, pageOptions, versionDescriptor);
                return true;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

        var searchOptions = new Dotnet.AzureDevOps.Core.Search.Options.WikiSearchOptions
        {
            SearchText = "Searchable",
            Project = [_azureDevOpsConfiguration.ProjectName],
            Wiki = [wikiCreateOptions.Name],
            IncludeFacets = false,
            Skip = 0,
            Top = 1
        };

        AzureDevOpsActionResult<string> result = await _searchClient.SearchWikiAsync(searchOptions);
        Assert.False(string.IsNullOrEmpty(result.Value));
    }

    /// <summary>
    /// Requires code search to be enabled in Azure DevOps organization settings.
    /// Pre installation required in azure DevOps https://marketplace.visualstudio.com/items?itemName=ms.vss-code-search
    /// Requires code with the text to find committed.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CodeSearch_ReturnsResultsAsync()
    {
        AzureDevOpsActionResult<bool> codeSearchEnabled = await _searchClient.IsCodeSearchEnabledAsync();

        var codeSearchOptions = new CodeSearchOptions
        {
            SearchText = "var findme",
            Project = [_azureDevOpsConfiguration.ProjectName],
            Repository = [_azureDevOpsConfiguration.RepoName],
            Branch = [_azureDevOpsConfiguration.MainBranchName],
            IncludeFacets = true,
            Skip = 0,
            Top = 1
        };
        try
        {
            Core.Common.AzureDevOpsActionResult<string> result = await _searchClient.SearchCodeAsync(codeSearchOptions);
            Assert.Contains("codesearch.cs", result.Value);
        }
        catch(HttpRequestException ex) when(ex.StatusCode == HttpStatusCode.NotFound)
        {
            if(codeSearchEnabled.Value)
            {
                throw new Exception("Code search is enabled but returned 404 Not Found.", ex);
            }
            else
            {
                Assert.True(true, "Code search is not enabled, as expected.");
            }
        }
        catch
        {
            throw;
        }
    }

    [Fact]
    public async Task WorkItemSearch_ReturnsResultsAsync()
    {
        string title = $"search-workitem-{UtcStamp()}";
        int? workItemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions
        {
            Title = title,
            Description = "search work item",
            Tags = "IntegrationTest"
        });
        Assert.True(workItemId.HasValue);
        _createdWorkItemIds.Add(workItemId!.Value);

        var searchOptions = new WorkItemSearchOptions
        {
            SearchText = title,
            Project = [_azureDevOpsConfiguration.ProjectName],
            WorkItemType = ["Task"],
            IncludeFacets = false,
            Skip = 0,
            Top = 1
        };

        AzureDevOpsActionResult<string> result = await _searchClient.SearchWorkItemsAsync(searchOptions);
        Assert.False(string.IsNullOrEmpty(result.Value));
    }

    [Fact]
    public async Task IsCodeSearchEnabledAsync_ReturnsBooleanAsync()
    {
        AzureDevOpsActionResult<bool> result = await _searchClient.IsCodeSearchEnabledAsync();

        Assert.True(result.IsSuccessful, $"Expected IsCodeSearchEnabledAsync to succeed, but got error: {result.ErrorMessage}");
        Assert.True(result.Value == true || result.Value == false, "Result should be a boolean value.");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach(Guid id in _createdWikis.AsEnumerable().Reverse())
        {
            await _wikiClient.DeleteWikiAsync(id);
        }

        foreach(int id in _createdWorkItemIds.AsEnumerable().Reverse())
        {
            await _workItemsClient.DeleteWorkItemAsync(id);
        }
    }

    private static string UtcStamp() =>
        DateTime.UtcNow.ToString("O").Replace(':', '-');
}
