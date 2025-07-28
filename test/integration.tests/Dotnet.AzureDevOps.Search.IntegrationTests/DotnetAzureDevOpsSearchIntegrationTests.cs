using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.Search.Options;
using Dotnet.AzureDevOps.Tests.Common;

namespace Dotnet.AzureDevOps.Search.IntegrationTests;

[ExcludeFromCodeCoverage]
public class DotnetAzureDevOpsSearchIntegrationTests
{
    private readonly ISearchClient _searchClient;

    public DotnetAzureDevOpsSearchIntegrationTests()
    {
        AzureDevOpsConfiguration config = new AzureDevOpsConfiguration();
        _searchClient = new SearchClient(config.OrganisationUrl, config.PersonalAccessToken);
    }

    [Fact]
    public async Task CodeSearch_ReturnsResultAsync()
    {
        var options = new CodeSearchOptions { SearchText = "test", Skip = 0, Top = 1 };
        string result = await _searchClient.SearchCodeAsync(options);
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task WikiSearch_ReturnsResultAsync()
    {
        var options = new WikiSearchOptions { SearchText = "home", Skip = 0, Top = 1 };
        string result = await _searchClient.SearchWikiAsync(options);
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task WorkItemSearch_ReturnsResultAsync()
    {
        var options = new WorkItemSearchOptions { SearchText = "bug", Skip = 0, Top = 1 };
        string result = await _searchClient.SearchWorkItemsAsync(options);
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
