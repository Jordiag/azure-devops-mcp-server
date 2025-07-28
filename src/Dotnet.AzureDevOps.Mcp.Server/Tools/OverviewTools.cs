using System.ComponentModel;
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
    public static Task<Guid> CreateWikiAsync(string organizationUrl, string projectName, string personalAccessToken, WikiCreateOptions options)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateWikiAsync(options);
    }

    [McpServerTool, Description("Retrieves a wiki by identifier.")]
    public static Task<WikiV2?> GetWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.GetWikiAsync(wikiId);
    }

    [McpServerTool, Description("Lists wikis in the project.")]
    public static Task<IReadOnlyList<WikiV2>> ListWikisAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.ListWikisAsync();
    }

    [McpServerTool, Description("Deletes a wiki.")]
    public static Task DeleteWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteWikiAsync(wikiId);
    }

    [McpServerTool, Description("Creates or updates a wiki page.")]
    public static Task<int?> CreateOrUpdatePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor version)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateOrUpdatePageAsync(wikiId, options, version);
    }

    [McpServerTool, Description("Retrieves a wiki page.")]
    public static Task<WikiPageResponse?> GetPageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPageAsync(wikiId, path);
    }

    [McpServerTool, Description("Deletes a wiki page.")]
    public static Task DeletePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, GitVersionDescriptor version)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePageAsync(wikiId, path, version);
    }

    [McpServerTool, Description("Lists pages in a wiki.")]
    public static Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPagesBatchOptions options)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPagesAsync(wikiId, options);
    }

    [McpServerTool, Description("Gets wiki page content.")]
    public static Task<string?> GetPageTextAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPageTextAsync(wikiId, path);
    }

    [McpServerTool, Description("Searches wikis for text.")]
    public static Task<string> SearchWikiAsync(string organizationUrl, string projectName, string personalAccessToken, WikiSearchOptions options)
    {
        WikiClient client = CreateWikiClient(organizationUrl, projectName, personalAccessToken);
        return client.SearchWikiAsync(options);
    }

    [McpServerTool, Description("Retrieves project summary information.")]
    public static Task<TeamProject?> GetProjectSummaryAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        SummaryClient client = CreateSummaryClient(organizationUrl, projectName, personalAccessToken);
        return client.GetProjectSummaryAsync();
    }

    [McpServerTool, Description("Lists dashboards under the project.")]
    public static Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        DashboardClient client = CreateDashboardClient(organizationUrl, projectName, personalAccessToken);
        return client.ListDashboardsAsync();
    }

    [McpServerTool, Description("Retrieves a dashboard by identifier.")]
    public static Task<Dashboard?> GetDashboardAsync(string organizationUrl, string projectName, string personalAccessToken, Guid dashboardId)
    {
        DashboardClient client = CreateDashboardClient(organizationUrl, projectName, personalAccessToken);
        return client.GetDashboardAsync(dashboardId);
    }
}
