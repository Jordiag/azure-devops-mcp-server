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

    [McpServerTool, Description("Searches for source code files within Azure DevOps repositories using full-text search. Supports filtering by project name, repository name, file path, and branch. Returns JSON containing search results with file paths, line matches, and code snippets. Requires the Code Search extension to be enabled in the Azure DevOps organization.")]
    public async Task<string> SearchCodeAsync(CodeSearchOptions options)
    {
        return (await _searchClient.SearchCodeAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Searches for content within Azure DevOps wiki pages using full-text search. Searches across wiki page titles and content, supports filtering by project and specific wiki names. Returns JSON containing matching wiki pages with titles, paths, and content snippets.")]
    public async Task<string> SearchWikiAsync(WikiSearchOptions options)
    {
        return (await _searchClient.SearchWikiAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Searches for Azure DevOps work items (tasks, user stories, bugs, epics, etc.) using full-text search across titles, descriptions, and comments. Supports filtering by project, area path, work item type, state, and assigned user. Returns JSON containing matching work items with their details, fields, and metadata.")]
    public async Task<string> SearchWorkItemsAsync(WorkItemSearchOptions options)
    {
        return (await _searchClient.SearchWorkItemsAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Checks whether the Azure DevOps Code Search extension is installed and enabled for the organization. Code search functionality requires this extension to be installed from the Azure DevOps marketplace. Returns true if enabled, false otherwise.")]
    public async Task<bool> IsCodeSearchEnabledAsync()
    {
        return (await _searchClient.IsCodeSearchEnabledAsync()).EnsureSuccess(_logger);
    }
}
