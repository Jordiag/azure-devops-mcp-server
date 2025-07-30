using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
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
[Component(Component.Overview)]
public class DotnetAzureDevOpsSearchIntegrationTests : IAsyncLifetime
{
    private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
    private readonly WikiClient _wikiClient;
    private readonly WorkItemsClient _workItemsClient;
    private readonly SearchClient _searchClient;
    private readonly List<Guid> _createdWikis = [];
    private readonly List<int> _createdWorkItemIds = [];

    public DotnetAzureDevOpsSearchIntegrationTests()
    {
        _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

        _wikiClient = new WikiClient(
            _azureDevOpsConfiguration.OrganisationUrl,
            _azureDevOpsConfiguration.ProjectName,
            _azureDevOpsConfiguration.PersonalAccessToken);

        _workItemsClient = new WorkItemsClient(
            _azureDevOpsConfiguration.OrganisationUrl,
            _azureDevOpsConfiguration.ProjectName,
            _azureDevOpsConfiguration.PersonalAccessToken);

        _searchClient = new SearchClient(
            _azureDevOpsConfiguration.Organisation,
            _azureDevOpsConfiguration.PersonalAccessToken);
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

        await _wikiClient.CreateOrUpdatePageAsync(wikiId, pageOptions, versionDescriptor);

        var searchOptions = new Dotnet.AzureDevOps.Core.Search.Options.WikiSearchOptions
        {
            SearchText = "Searchable",
            Project = [_azureDevOpsConfiguration.ProjectName],
            Wiki = [wikiCreateOptions.Name],
            IncludeFacets = false,
            Skip = 0,
            Top = 1
        };

        string result = await _searchClient.SearchWikiAsync(searchOptions);
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task CodeSearch_ReturnsResultsAsync()
    {
        var codeSearchOptions = new CodeSearchOptions
        {
            SearchText = "Azure DevOps",
            Project = [_azureDevOpsConfiguration.ProjectName],
            Repository = [_azureDevOpsConfiguration.RepoName],
            Branch = [_azureDevOpsConfiguration.MainBranchName],
            IncludeFacets = false,
            Skip = 0,
            Top = 1
        };

        string result = await _searchClient.SearchCodeAsync(codeSearchOptions);
        Assert.False(string.IsNullOrEmpty(result));
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

        string result = await _searchClient.SearchWorkItemsAsync(searchOptions);
        Assert.False(string.IsNullOrEmpty(result));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (Guid id in _createdWikis.AsEnumerable().Reverse())
        {
            await _wikiClient.DeleteWikiAsync(id);
        }

        foreach (int id in _createdWorkItemIds.AsEnumerable().Reverse())
        {
            await _workItemsClient.DeleteWorkItemAsync(id);
        }
    }

    private static string UtcStamp() =>
        DateTime.UtcNow.ToString("O").Replace(':', '-');
}
