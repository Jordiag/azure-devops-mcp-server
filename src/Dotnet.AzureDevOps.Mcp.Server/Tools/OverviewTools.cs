using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Wiki operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class OverviewTools
{
    private static WikiClient CreateWikiClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    private static SummaryClient CreateSummaryClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    private static DashboardClient CreateDashboardClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    [McpServerTool, Description("Creates a new wiki.")]
    public static async Task<Guid> CreateWikiAsync(string organizationUrl, string projectName, string personalAccessToken, WikiCreateOptions options, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateWikiAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a wiki by identifier.")]
    public static async Task<WikiV2> GetWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetWikiAsync(wikiId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists wikis in the project.")]
    public static async Task<IReadOnlyList<WikiV2>> ListWikisAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListWikisAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a wiki.")]
    public static async Task DeleteWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, ILogger? logger = null)
    {
        (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteWikiAsync(wikiId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates or updates a wiki page.")]
    public static async Task<int> CreateOrUpdatePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor version, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateOrUpdatePageAsync(wikiId, options, version)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a wiki page.")]
    public static async Task<WikiPageResponse> GetPageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetPageAsync(wikiId, path)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a wiki page.")]
    public static async Task DeletePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, GitVersionDescriptor version, ILogger? logger = null)
    {
        (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeletePageAsync(wikiId, path, version)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pages in a wiki.")]
    public static async Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPagesBatchOptions options, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPagesAsync(wikiId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets wiki page content.")]
    public static async Task<string> GetPageTextAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, ILogger? logger = null)
    {
        return (await CreateWikiClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetPageTextAsync(wikiId, path)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves project summary information.")]
    public static async Task<TeamProject> GetProjectSummaryAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateSummaryClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetProjectSummaryAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Lists dashboards under the project.")]
    public static async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateDashboardClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListDashboardsAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a dashboard by identifier and team name.")]
    public static async Task<Dashboard> GetDashboardAsync(string organizationUrl, string projectName, string personalAccessToken, Guid dashboardId, string teamName, ILogger? logger = null)
    {
        return (await CreateDashboardClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetDashboardAsync(dashboardId, teamName)).EnsureSuccess();
    }
}