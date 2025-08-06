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
public static class SearchTools
{
    private static SearchClient CreateClient(string organization, string personalAccessToken, ILogger? logger = null)
        => new(organization, personalAccessToken, logger);

    [McpServerTool, Description("Searches code in a project or repository.")]
    public static async Task<string> SearchCodeAsync(string organization, string personalAccessToken, CodeSearchOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organization, personalAccessToken, logger)
            .SearchCodeAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Searches wiki pages.")]
    public static async Task<string> SearchWikiAsync(string organization, string personalAccessToken, WikiSearchOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organization, personalAccessToken, logger)
            .SearchWikiAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Searches work items.")]
    public static async Task<string> SearchWorkItemsAsync(string organization, string personalAccessToken, WorkItemSearchOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organization, personalAccessToken, logger)
            .SearchWorkItemsAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Checks if code search is enabled.")]
    public static async Task<bool> IsCodeSearchEnabledAsync(string organization, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateClient(organization, personalAccessToken, logger)
            .IsCodeSearchEnabledAsync()).EnsureSuccess();
    }
}