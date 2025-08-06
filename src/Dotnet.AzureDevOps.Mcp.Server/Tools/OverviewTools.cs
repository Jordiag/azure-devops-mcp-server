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
/// Exposes Wiki and Overview operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class OverviewTools
{
    private readonly IWikiClient _wikiClient;
    private readonly ISummaryClient _summaryClient;
    private readonly IDashboardClient _dashboardClient;
    private readonly ILogger<OverviewTools> _logger;

    public OverviewTools(IWikiClient wikiClient, ISummaryClient summaryClient, IDashboardClient dashboardClient, ILogger<OverviewTools> logger)
    {
        _wikiClient = wikiClient;
        _summaryClient = summaryClient;
        _dashboardClient = dashboardClient;
        _logger = logger;
    }

    [McpServerTool, Description("Creates a new wiki in the Azure DevOps project. A wiki can be either a Code Wiki (backed by a Git repository) or Project Wiki (standalone). Requires name, project ID, repository ID for code wikis, and branch information. Returns the unique wiki ID.")]
    public async Task<Guid> CreateWikiAsync(WikiCreateOptions options)
    {
        return (await _wikiClient.CreateWikiAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves detailed information about a specific wiki including its name, type (Code/Project), repository details, and configuration. The wiki must exist and be accessible to the current user.")]
    public async Task<WikiV2> GetWikiAsync(Guid wikiId)
    {
        return (await _wikiClient.GetWikiAsync(wikiId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists all wikis in the Azure DevOps project that the current user has access to. Returns basic information about each wiki including name, ID, type, and associated repository information for code wikis.")]
    public async Task<IReadOnlyList<WikiV2>> ListWikisAsync()
    {
        return (await _wikiClient.ListWikisAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Permanently deletes a wiki and all its pages from Azure DevOps. For code wikis, this removes the wiki configuration but not the underlying repository. This action cannot be undone. Returns the deleted wiki information.")]
    public async Task<WikiV2> DeleteWikiAsync(Guid wikiId)
    {
        return (await _wikiClient.DeleteWikiAsync(wikiId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a new wiki page or updates an existing page with new content. Requires the page path, Markdown content, and version (ETag) for updates to prevent conflicts. For code wikis, changes are committed to the underlying Git repository. Returns the page ID.")]
    public async Task<int> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor gitVersionDescriptor)
    {
        return (await _wikiClient.CreateOrUpdatePageAsync(wikiId, options, gitVersionDescriptor)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a specific wiki page including its content, metadata, ETag for version control, and page hierarchy information. The page path should include the .md extension (e.g., '/Home.md').")]
    public async Task<WikiPageResponse> GetPageAsync(Guid wikiId, string path)
    {
        return (await _wikiClient.GetPageAsync(wikiId, path)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a wiki page.")]
    public async Task<WikiPageResponse> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor)
    {
        return (await _wikiClient.DeletePageAsync(wikiId, path, gitVersionDescriptor)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists wiki pages.")]
    public async Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions options, GitVersionDescriptor? versionDescriptor = null)
    {
        return (await _wikiClient.ListPagesAsync(wikiId, options, versionDescriptor)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets wiki page text content.")]
    public async Task<string> GetPageTextAsync(Guid wikiId, string path)
    {
        return (await _wikiClient.GetPageTextAsync(wikiId, path)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets project summary information.")]
    public async Task<TeamProject> GetProjectSummaryAsync()
    {
        return (await _summaryClient.GetProjectSummaryAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a specific dashboard configuration including its widgets, layout, and settings. Dashboards provide visual summaries of project metrics, build status, work items, and other key project data. Requires dashboard ID and team name.")]
    public async Task<Dashboard> GetDashboardAsync(Guid dashboardId, string teamName)
    {
        return (await _dashboardClient.GetDashboardAsync(dashboardId, teamName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists all dashboards available in the Azure DevOps project, including team dashboards and project-level dashboards. Returns dashboard metadata such as names, owners, and basic configuration. Useful for discovering available project overview screens.")]
    public async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync()
    {
        return (await _dashboardClient.ListDashboardsAsync()).EnsureSuccess(_logger);
    }
}
