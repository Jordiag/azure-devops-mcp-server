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
public class ArtifactsTools(IArtifactsClient artifactsClient, ILogger<ArtifactsTools> logger)
{
    private readonly IArtifactsClient _artifactsClient = artifactsClient;
    private readonly ILogger<ArtifactsTools> _logger = logger;

    [McpServerTool, Description("Creates a new package feed in Azure DevOps Artifacts. A feed is a container for storing and managing packages (NuGet, npm, Maven, etc.). Requires a name and optional description. Feeds support versioning, access control, and upstream sources. Returns the unique feed ID.")]
    public async Task<Guid> CreateFeedAsync(FeedCreateOptions options, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.CreateFeedAsync(options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Updates an existing package feed's properties such as name, description, or visibility settings. This modifies the feed metadata but does not affect stored packages. Returns true if the update was successful.")]
    public async Task<bool> UpdateFeedAsync(Guid feedId, FeedUpdateOptions options, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.UpdateFeedAsync(feedId, options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific package feed including its name, description, URL, capabilities, and creation details. The feed must exist and the caller must have read permissions.")]
    public async Task<Feed> GetFeedAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.GetFeedAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all package feeds in the Azure DevOps project that the current user has access to. Returns basic information about each feed including name, ID, description, and capabilities. Useful for discovering available package repositories.")]
    public async Task<IReadOnlyList<Feed>> ListFeedsAsync(CancellationToken cancellationToken = default) =>
        (await _artifactsClient.ListFeedsAsync(cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a package feed and all its packages from Azure DevOps Artifacts. This action cannot be undone and will break any builds or applications depending on packages in this feed. Returns true if deletion was successful.")]
    public async Task<bool> DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.DeleteFeedAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all packages stored in a specific feed. Returns package metadata including names, versions, download counts, and publish dates. An empty list is returned for new feeds with no packages. Useful for inventory management and package discovery.")]
    public async Task<IReadOnlyList<Package>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.ListPackagesAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific version of a package in a feed. Returns package metadata, dependencies, size, and version-specific details. The package and version must exist in the specified feed.")]
    public async Task<Package> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.GetPackageVersionAsync(feedId, packageName, version, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Updates metadata and properties of a specific package version such as listing status, tags, or description. Does not modify the package contents. Returns true if the update was successful.")]
    public async Task<bool> UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.UpdatePackageVersionAsync(feedId, packageName, version, details, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a specific version of a package from a feed. This action cannot be undone and may break builds or applications depending on this package version. Returns true if deletion was successful.")]
    public async Task<bool> DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.DeletePackageAsync(feedId, packageName, version, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Downloads the binary content of a specific package version from a feed. Returns a stream containing the package file (e.g., .nupkg, .tgz). The caller is responsible for disposing the stream after use.")]
    public async Task<Stream> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.DownloadPackageAsync(feedId, packageName, version, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the access permissions for a feed, showing which users, groups, or service principals have read, contribute, or administrator rights. Returns permission entries with identity and role information.")]
    public async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.GetFeedPermissionsAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new view for a package feed. Feed views are filtered subsets of packages (e.g., 'Release' view showing only stable versions, 'Prerelease' showing all versions). Views help organize packages by quality or lifecycle stage. Returns the created view.")]
    public async Task<FeedView> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.CreateFeedViewAsync(feedId, feedView, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all views configured for a specific feed. Views provide filtered perspectives of feed packages (e.g., 'Release', 'Prerelease'). Returns view metadata including names, visibility, and filtering criteria.")]
    public async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.ListFeedViewsAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a view from a feed. This removes the filtered perspective but does not affect the underlying packages. Applications using the view URL will no longer be able to access packages through this view. Returns true if deletion was successful.")]
    public async Task<bool> DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.DeleteFeedViewAsync(feedId, viewId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Controls how a specific package behaves with upstream sources (e.g., NuGet.org, npmjs.com). Can allow, block, or use external caching. This determines whether the feed will fetch packages from external sources when not found locally. Returns true if setting was successful.")]
    public async Task<bool> SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.SetUpstreamingBehaviorAsync(feedId, packageName, behavior, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the current upstreaming behavior configuration for a specific package, indicating how it interacts with upstream sources like NuGet.org or npmjs.com. Returns the behavior setting (Allow, Block, or UseExternalCache).")]
    public async Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.GetUpstreamingBehaviorAsync(feedId, packageName, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the current retention policy for a feed, which defines how long packages are kept, maximum package count, and how recently downloaded packages are handled. Helps manage feed storage costs and cleanup old packages.")]
    public async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.GetRetentionPolicyAsync(feedId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Sets or updates the retention policy for a feed, controlling automatic cleanup of old packages. Policies can limit by age (days), count, or protect recently downloaded packages. Helps manage storage costs and maintain feed performance. Returns the updated policy.")]
    public async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default) =>
        (await _artifactsClient.SetRetentionPolicyAsync(feedId, policy, cancellationToken)).EnsureSuccess(_logger);
}
