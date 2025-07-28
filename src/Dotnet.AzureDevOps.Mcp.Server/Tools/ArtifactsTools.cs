using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Artifacts operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ArtifactsTools
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

    [McpServerTool, Description("Gets permissions for a feed.")]
    public static Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetFeedPermissionsAsync(feedId);
    }

    [McpServerTool, Description("Sets permissions on a feed.")]
    public static Task SetFeedPermissionsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, IEnumerable<FeedPermission> permissions)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SetFeedPermissionsAsync(feedId, permissions);
    }

    [McpServerTool, Description("Creates a feed view.")]
    public static Task<FeedView> CreateFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedView view)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateFeedViewAsync(feedId, view);
    }

    [McpServerTool, Description("Lists feed views.")]
    public static Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListFeedViewsAsync(feedId);
    }

    [McpServerTool, Description("Deletes a feed view.")]
    public static Task DeleteFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string viewId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteFeedViewAsync(feedId, viewId);
    }

    [McpServerTool, Description("Sets upstreaming behavior for a package.")]
    public static Task SetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, UpstreamingBehavior behavior)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SetUpstreamingBehaviorAsync(feedId, packageName, behavior);
    }

    [McpServerTool, Description("Gets upstreaming behavior for a package.")]
    public static Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetUpstreamingBehaviorAsync(feedId, packageName);
    }

    [McpServerTool, Description("Gets a specific package version.")]
    public static Task<Package> GetPackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPackageVersionAsync(feedId, packageName, version);
    }

    [McpServerTool, Description("Updates metadata for a package version.")]
    public static Task UpdatePackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version, PackageVersionDetails details)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdatePackageVersionAsync(feedId, packageName, version, details);
    }

    [McpServerTool, Description("Downloads a package version.")]
    public static Task<Stream> DownloadPackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DownloadPackageAsync(feedId, packageName, version);
    }

    [McpServerTool, Description("Gets the retention policy for a feed.")]
    public static Task<FeedRetentionPolicy> GetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetRetentionPolicyAsync(feedId);
    }

    [McpServerTool, Description("Sets the retention policy for a feed.")]
    public static Task<FeedRetentionPolicy> SetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedRetentionPolicy policy)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SetRetentionPolicyAsync(feedId, policy);
    }
}