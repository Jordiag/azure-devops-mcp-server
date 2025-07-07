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
public static class ArtifactsTools
{
    private static ArtifactsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new feed.")]
    public static Task<Guid> CreateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, FeedCreateOptions options)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateFeedAsync(options);
    }

    [McpServerTool, Description("Updates an existing feed.")]
    public static Task UpdateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedUpdateOptions options)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdateFeedAsync(feedId, options);
    }

    [McpServerTool, Description("Retrieves a feed by identifier.")]
    public static Task<Feed?> GetFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetFeedAsync(feedId);
    }

    [McpServerTool, Description("Lists all feeds in the project.")]
    public static Task<IReadOnlyList<Feed>> ListFeedsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListFeedsAsync();
    }

    [McpServerTool, Description("Deletes a feed by identifier.")]
    public static Task DeleteFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteFeedAsync(feedId);
    }

    [McpServerTool, Description("Lists packages within a feed.")]
    public static Task<IReadOnlyList<Package>> ListPackagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPackagesAsync(feedId);
    }

    [McpServerTool, Description("Deletes a package from a feed.")]
    public static Task DeletePackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePackageAsync(feedId, packageName, version);
    }
}
