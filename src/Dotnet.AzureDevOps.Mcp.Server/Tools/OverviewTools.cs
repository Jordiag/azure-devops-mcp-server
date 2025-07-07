using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Overview;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Wiki operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class OverviewTools
{
    private static WikiClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new wiki.")]
    public static Task<Guid> CreateWikiAsync(string organizationUrl, string projectName, string personalAccessToken, WikiCreateOptions options)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateWikiAsync(options);
    }

    [McpServerTool, Description("Retrieves a wiki by identifier.")]
    public static Task<WikiV2?> GetWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetWikiAsync(wikiId);
    }

    [McpServerTool, Description("Lists wikis in the project.")]
    public static Task<IReadOnlyList<WikiV2>> ListWikisAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListWikisAsync();
    }

    [McpServerTool, Description("Deletes a wiki.")]
    public static Task DeleteWikiAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteWikiAsync(wikiId);
    }

    [McpServerTool, Description("Creates or updates a wiki page.")]
    public static Task<int?> CreateOrUpdatePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, WikiPageUpdateOptions options, GitVersionDescriptor version)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateOrUpdatePageAsync(wikiId, options, version);
    }

    [McpServerTool, Description("Retrieves a wiki page.")]
    public static Task<WikiPageResponse?> GetPageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPageAsync(wikiId, path);
    }

    [McpServerTool, Description("Deletes a wiki page.")]
    public static Task DeletePageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid wikiId, string path, GitVersionDescriptor version)
    {
        WikiClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePageAsync(wikiId, path, version);
    }
}
