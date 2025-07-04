using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Artifacts operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ArtifactsTools
{
    private ArtifactsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new feed.")]
    public Task<Guid> CreateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, FeedCreateOptions options)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateFeedAsync(options);
    }

    [McpServerTool, Description("Updates an existing feed.")]
    public Task UpdateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedUpdateOptions options)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdateFeedAsync(feedId, options);
    }

    [McpServerTool, Description("Retrieves a feed by identifier.")]
    public Task<Feed?> GetFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetFeedAsync(feedId);
    }

    [McpServerTool, Description("Lists all feeds in the project.")]
    public Task<IReadOnlyList<Feed>> ListFeedsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListFeedsAsync();
    }

    [McpServerTool, Description("Deletes a feed by identifier.")]
    public Task DeleteFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteFeedAsync(feedId);
    }

    [McpServerTool, Description("Lists packages within a feed.")]
    public Task<IReadOnlyList<Package>> ListPackagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPackagesAsync(feedId);
    }

    [McpServerTool, Description("Deletes a package from a feed.")]
    public Task DeletePackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePackageAsync(feedId, packageName, version);
    }
}
