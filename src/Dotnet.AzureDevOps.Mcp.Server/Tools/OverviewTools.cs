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
public class OverviewTools(IWikiClient wikiClient, ISummaryClient summaryClient, IDashboardClient dashboardClient, ILogger<OverviewTools> logger)
{
    private readonly IWikiClient _wikiClient = wikiClient;
    private readonly ISummaryClient _summaryClient = summaryClient;
    private readonly IDashboardClient _dashboardClient = dashboardClient;
    private readonly ILogger<OverviewTools> _logger = logger;

    [McpServerTool, Description("Creates a new wiki in the Azure DevOps project. A wiki can be either a Code Wiki (backed by a Git repository) or Project Wiki (standalone). Requires name, project ID, repository ID for code wikis, and branch information. Returns the unique wiki ID.")]
    public async Task<Guid> CreateWikiAsync(WikiCreateOptions options, CancellationToken cancellationToken = default) =>
        (await _wikiClient.CreateWikiAsync(options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific wiki including its name, type (Code/Project), repository details, and configuration. The wiki must exist and be accessible to the current user.")]
    public async Task<WikiV2> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default) =>
        (await _wikiClient.GetWikiAsync(wikiId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all wikis in the Azure DevOps project that the current user has access to. Returns basic information about each wiki including name, ID, type, and associated repository information for code wikis.")]
    public async Task<IReadOnlyList<WikiV2>> ListWikisAsync(CancellationToken cancellationToken = default) =>
        (await _wikiClient.ListWikisAsync(cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a wiki and all its pages from Azure DevOps. For code wikis, this removes the wiki configuration but not the underlying repository. This action cannot be undone. Returns the deleted wiki information.")]
    public async Task<WikiV2> DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default) =>
        (await _wikiClient.DeleteWikiAsync(wikiId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new wiki page or updates an existing page with new content. Requires the page path, Markdown content, and version (ETag) for updates to prevent conflicts. For code wikis, changes are committed to the underlying Git repository. Returns the page ID.")]
    public async Task<int> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default) =>
        (await _wikiClient.CreateOrUpdatePageAsync(wikiId, options, gitVersionDescriptor, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves a specific wiki page including its content, metadata, ETag for version control, and page hierarchy information. The page path should include the .md extension (e.g., '/Home.md').")]
    public async Task<WikiPageResponse> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default) =>
        (await _wikiClient.GetPageAsync(wikiId, path, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a wiki page and its content from Azure DevOps. For code wikis, this creates a commit in the underlying Git repository removing the page file. This action cannot be undone and will break any links to the deleted page. Returns information about the deleted page.")]
    public async Task<WikiPageResponse> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default) =>
        (await _wikiClient.DeletePageAsync(wikiId, path, gitVersionDescriptor, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all pages within a wiki, including their paths, titles, and hierarchy structure. Supports filtering and pagination options to manage large wikis efficiently. Returns page metadata without content, useful for navigation, indexing, or bulk operations on wiki pages.")]
    public async Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions options, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default) => 
        (await _wikiClient.ListPagesAsync(wikiId, options, versionDescriptor, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the raw text content of a wiki page without metadata or formatting. Returns the Markdown source content that can be used for editing, parsing, or content analysis. The page path should include the .md extension for proper identification.")]
    public async Task<string> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default) =>
        (await _wikiClient.GetPageTextAsync(wikiId, path, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves comprehensive project summary information including name, description, state, visibility settings, capabilities, and basic statistics. Provides a high-level overview of the Azure DevOps project configuration and current status for administrative and reporting purposes.")]
    public async Task<TeamProject> GetProjectSummaryAsync(CancellationToken cancellationToken = default) =>
        (await _summaryClient.GetProjectSummaryAsync(cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves a specific dashboard configuration including its widgets, layout, and settings. Dashboards provide visual summaries of project metrics, build status, work items, and other key project data. Requires dashboard ID and team name.")]
    public async Task<Dashboard> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default) =>
        (await _dashboardClient.GetDashboardAsync(dashboardId, teamName, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all dashboards available in the Azure DevOps project, including team dashboards and project-level dashboards. Returns dashboard metadata such as names, owners, and basic configuration. Useful for discovering available project overview screens.")]
    public async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(CancellationToken cancellationToken = default) 
        => (await _dashboardClient.ListDashboardsAsync(cancellationToken)).EnsureSuccess(_logger);
}
