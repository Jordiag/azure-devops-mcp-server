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

    [McpServerTool, Description("Creates a new wiki.")]
    public async Task<Guid> CreateWikiAsync(WikiCreateOptions options)
    {
        return (await _wikiClient.CreateWikiAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a wiki by identifier.")]
    public async Task<WikiV2> GetWikiAsync(Guid wikiId)
    {
        return (await _wikiClient.GetWikiAsync(wikiId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists wikis in the project.")]
    public async Task<IReadOnlyList<WikiV2>> ListWikisAsync()
    {
        return (await _wikiClient.ListWikisAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a wiki.")]
    public async Task<WikiV2> DeleteWikiAsync(Guid wikiId)
    {
        return (await _wikiClient.DeleteWikiAsync(wikiId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates or updates a wiki page.")]
    public async Task<int> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor gitVersionDescriptor)
    {
        return (await _wikiClient.CreateOrUpdatePageAsync(wikiId, options, gitVersionDescriptor)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a wiki page.")]
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

    [McpServerTool, Description("Retrieves a dashboard.")]
    public async Task<Dashboard> GetDashboardAsync(Guid dashboardId, string teamName)
    {
        return (await _dashboardClient.GetDashboardAsync(dashboardId, teamName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists dashboards.")]
    public async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync()
    {
        return (await _dashboardClient.ListDashboardsAsync()).EnsureSuccess(_logger);
    }
}
