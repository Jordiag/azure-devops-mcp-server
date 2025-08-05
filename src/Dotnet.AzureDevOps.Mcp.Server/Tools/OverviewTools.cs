using System;
using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Wiki operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class OverviewTools
{
    private static WikiClient CreateWikiClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    private static SummaryClient CreateSummaryClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    private static DashboardClient CreateDashboardClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new wiki.")]
    public static async Task<Guid> CreateWikiAsync(string organizationUrl, string projectName, string personalAccessToken, WikiCreateOptions options)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Guid> result = await client.CreateWikiAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage);
        return result.Value;
    }

    [McpServerTool, Description("Retrieves a wiki by identifier.")]
    public static async Task<WikiV2?> GetWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<WikiV2> result = await client.GetWikiAsync(wikiId);
        return result.Value;
    }

    [McpServerTool, Description("Lists wikis in the project.")]
    public static async Task<IReadOnlyList<WikiV2>> ListWikisAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<WikiV2>> result = await client.ListWikisAsync();
        return result.Value;
    }

    [McpServerTool, Description("Deletes a wiki.")]
    public static async Task DeleteWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<WikiV2> result = await client.DeleteWikiAsync(wikiId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    [McpServerTool, Description("Creates or updates a wiki page.")]
    public static async Task<int?> CreateOrUpdatePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor version)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<int> result = await client.CreateOrUpdatePageAsync(wikiId, options, version);
        return result.IsSuccessful ? result.Value : null;
    }

    [McpServerTool, Description("Retrieves a wiki page.")]
    public static async Task<WikiPageResponse?> GetPageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<WikiPageResponse> result = await client.GetPageAsync(wikiId, path);
        return result.Value;
    }

    [McpServerTool, Description("Deletes a wiki page.")]
    public static async Task DeletePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, GitVersionDescriptor version)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<WikiPageResponse> result = await client.DeletePageAsync(wikiId, path, version);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    [McpServerTool, Description("Lists pages in a wiki.")]
    public static async Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPagesBatchOptions options)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>> result = await client.ListPagesAsync(wikiId, options);
        return result.Value;
    }

    [McpServerTool, Description("Gets wiki page content.")]
    public static async Task<string?> GetPageTextAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<string> result = await client.GetPageTextAsync(wikiId, path);
        return result.Value;
    }

    [McpServerTool, Description("Retrieves project summary information.")]
    public static async Task<TeamProject?> GetProjectSummaryAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        SummaryClient client = CreateSummaryClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<TeamProject> result = await client.GetProjectSummaryAsync();
        return result.Value;
    }

    [McpServerTool, Description("Lists dashboards under the project.")]
    public static async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        DashboardClient client = CreateDashboardClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<Dashboard>> result = await client.ListDashboardsAsync();
        return result.Value;
    }

    [McpServerTool, Description("Retrieves a dashboard by identifier and team name.")]
    public static async Task<Dashboard?> GetDashboardAsync(string organizationUrl, string projectName, string personalAccessToken, Guid dashboardId, string teamName)
    {
        DashboardClient client = CreateDashboardClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Dashboard> result = await client.GetDashboardAsync(dashboardId, teamName);
        return result.Value;
    }
}