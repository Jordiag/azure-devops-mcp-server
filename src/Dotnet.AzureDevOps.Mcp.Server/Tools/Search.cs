using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.Search.Options;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Search operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public static class SearchTools
{
    private static SearchClient CreateClient(string organization, string personalAccessToken)
        => new(organization, personalAccessToken);

    [McpServerTool, Description("Searches code in a project or repository.")]
    public static async Task<string> SearchCodeAsync(string organization, string personalAccessToken, CodeSearchOptions options)
    {
        return (await CreateClient(organization, personalAccessToken)
            .SearchCodeAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Searches wiki pages.")]
    public static async Task<string> SearchWikiAsync(string organization, string personalAccessToken, WikiSearchOptions options)
    {
        return (await CreateClient(organization, personalAccessToken)
            .SearchWikiAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Searches work items.")]
    public static async Task<string> SearchWorkItemsAsync(string organization, string personalAccessToken, WorkItemSearchOptions options)
    {
        return (await CreateClient(organization, personalAccessToken)
            .SearchWorkItemsAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Checks if code search is enabled.")]
    public static async Task<bool> IsCodeSearchEnabledAsync(string organization, string personalAccessToken)
    {
        return (await CreateClient(organization, personalAccessToken)
            .IsCodeSearchEnabledAsync()).EnsureSuccess();
    }
}