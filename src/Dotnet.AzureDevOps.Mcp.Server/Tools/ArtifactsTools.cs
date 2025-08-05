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
        => new ArtifactsClient(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new feed.")]
    public static async Task<Guid> CreateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, FeedCreateOptions options)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateFeedAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Updates an existing feed.")]
    public static async Task UpdateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedUpdateOptions options)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .UpdateFeedAsync(feedId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a feed by identifier.")]
    public static async Task<Feed> GetFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetFeedAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists all feeds in the project.")]
    public static async Task<IReadOnlyList<Feed>> ListFeedsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListFeedsAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a feed by identifier.")]
    public static async Task DeleteFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteFeedAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists packages within a feed.")]
    public static async Task<IReadOnlyList<Package>> ListPackagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPackagesAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a package from a feed.")]
    public static async Task DeletePackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeletePackageAsync(feedId, packageName, version)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets permissions for a feed.")]
    public static async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetFeedPermissionsAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a feed view.")]
    public static async Task<FeedView> CreateFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedView view)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateFeedViewAsync(feedId, view)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists feed views.")]
    public static async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListFeedViewsAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a feed view.")]
    public static async Task DeleteFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string viewId)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteFeedViewAsync(feedId, viewId)).EnsureSuccess();
    }

    [McpServerTool, Description("Sets upstreaming behavior for a package.")]
    public static async Task SetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, UpstreamingBehavior behavior)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .SetUpstreamingBehaviorAsync(feedId, packageName, behavior)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets upstreaming behavior for a package.")]
    public static async Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetUpstreamingBehaviorAsync(feedId, packageName)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a specific package version.")]
    public static async Task<Package> GetPackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetPackageVersionAsync(feedId, packageName, version)).EnsureSuccess();
    }

    [McpServerTool, Description("Updates metadata for a package version.")]
    public static async Task UpdatePackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version, PackageVersionDetails details)
    {
        (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .UpdatePackageVersionAsync(feedId, packageName, version, details)).EnsureSuccess();
    }

    [McpServerTool, Description("Downloads a package version.")]
    public static async Task<Stream> DownloadPackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DownloadPackageAsync(feedId, packageName, version)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets the retention policy for a feed.")]
    public static async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetRetentionPolicyAsync(feedId)).EnsureSuccess();
    }

    [McpServerTool, Description("Sets the retention policy for a feed.")]
    public static async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedRetentionPolicy policy)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .SetRetentionPolicyAsync(feedId, policy)).EnsureSuccess();
    }
}
