using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.Search.Options;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Search operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class SearchTools
{
    private readonly ISearchClient _searchClient;
    private readonly ILogger<SearchTools> _logger;

    public SearchTools(ISearchClient searchClient, ILogger<SearchTools> logger)
    {
        _searchClient = searchClient;
        _logger = logger;
    }

    [McpServerTool, Description("Searches code in a project or repository.")]
    public async Task<string> SearchCodeAsync(CodeSearchOptions options)
    {
        return (await _searchClient.SearchCodeAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Searches wiki pages.")]
    public async Task<string> SearchWikiAsync(WikiSearchOptions options)
    {
        return (await _searchClient.SearchWikiAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Searches work items.")]
    public async Task<string> SearchWorkItemsAsync(WorkItemSearchOptions options)
    {
        return (await _searchClient.SearchWorkItemsAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Checks if code search is enabled.")]
    public async Task<bool> IsCodeSearchEnabledAsync()
    {
        return (await _searchClient.IsCodeSearchEnabledAsync()).EnsureSuccess(_logger);
    }
}
