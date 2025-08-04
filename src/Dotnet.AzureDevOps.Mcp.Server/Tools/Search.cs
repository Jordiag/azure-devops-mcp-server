using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
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
    public static async Task<string?> SearchCodeAsync(string organization, string personalAccessToken, CodeSearchOptions options)
    {
        AzureDevOpsActionResult<string> result = await CreateClient(organization, personalAccessToken)
            .SearchCodeAsync(options);
        return !result.IsSuccessful ? throw new InvalidOperationException(result.ErrorMessage ?? "Failed to search code.") : result.Value;
    }

    [McpServerTool, Description("Searches wiki pages.")]
    public static async Task<string?> SearchWikiAsync(string organization, string personalAccessToken, WikiSearchOptions options)
    {
        AzureDevOpsActionResult<string> result = await CreateClient(organization, personalAccessToken)
            .SearchWikiAsync(options);
        return !result.IsSuccessful ? throw new InvalidOperationException(result.ErrorMessage ?? "Failed to search wiki.") : result.Value;
    }

    [McpServerTool, Description("Searches work items.")]
    public static async Task<string?> SearchWorkItemsAsync(string organization, string personalAccessToken, WorkItemSearchOptions options)
    {
        AzureDevOpsActionResult<string> result = await CreateClient(organization, personalAccessToken)
            .SearchWorkItemsAsync(options);
        return !result.IsSuccessful
            ? throw new InvalidOperationException(result.ErrorMessage ?? "Failed to search work items.")
            : result.Value;
    }

    [McpServerTool, Description("Checks if code search is enabled.")]
    public static async Task<bool> IsCodeSearchEnabledAsync(string organization, string personalAccessToken)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organization, personalAccessToken)
            .IsCodeSearchEnabledAsync();
        return !result.IsSuccessful
            ? throw new InvalidOperationException(result.ErrorMessage ?? "Failed to check code search status.")
            : result.Value;
    }
}