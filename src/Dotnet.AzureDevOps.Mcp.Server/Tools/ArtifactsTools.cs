using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Artifacts operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ArtifactsTools
{
    private readonly IArtifactsClient _artifactsClient;
    private readonly ILogger<ArtifactsTools> _logger;

    public ArtifactsTools(IArtifactsClient artifactsClient, ILogger<ArtifactsTools> logger)
    {
        _artifactsClient = artifactsClient;
        _logger = logger;
    }

    [McpServerTool, Description("Creates a new feed.")]
    public async Task<Guid> CreateFeedAsync(FeedCreateOptions options)
    {
        return (await _artifactsClient.CreateFeedAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Updates an existing feed.")]
    public async Task<bool> UpdateFeedAsync(Guid feedId, FeedUpdateOptions options)
    {
        return (await _artifactsClient.UpdateFeedAsync(feedId, options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a feed by identifier.")]
    public async Task<Feed> GetFeedAsync(Guid feedId)
    {
        return (await _artifactsClient.GetFeedAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists all feeds in the project.")]
    public async Task<IReadOnlyList<Feed>> ListFeedsAsync()
    {
        return (await _artifactsClient.ListFeedsAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a feed by identifier.")]
    public async Task<bool> DeleteFeedAsync(Guid feedId)
    {
        return (await _artifactsClient.DeleteFeedAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists packages in a feed.")]
    public async Task<IReadOnlyList<Package>> ListPackagesAsync(Guid feedId)
    {
        return (await _artifactsClient.ListPackagesAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a package version.")]
    public async Task<Package> GetPackageVersionAsync(Guid feedId, string packageName, string version)
    {
        return (await _artifactsClient.GetPackageVersionAsync(feedId, packageName, version)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Updates package version details.")]
    public async Task<bool> UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details)
    {
        return (await _artifactsClient.UpdatePackageVersionAsync(feedId, packageName, version, details)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a package.")]
    public async Task<bool> DeletePackageAsync(Guid feedId, string packageName, string version)
    {
        return (await _artifactsClient.DeletePackageAsync(feedId, packageName, version)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Downloads a package.")]
    public async Task<Stream> DownloadPackageAsync(Guid feedId, string packageName, string version)
    {
        return (await _artifactsClient.DownloadPackageAsync(feedId, packageName, version)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets feed permissions.")]
    public async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(Guid feedId)
    {
        return (await _artifactsClient.GetFeedPermissionsAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a feed view.")]
    public async Task<FeedView> CreateFeedViewAsync(Guid feedId, FeedView feedView)
    {
        return (await _artifactsClient.CreateFeedViewAsync(feedId, feedView)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists all views for a feed.")]
    public async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(Guid feedId)
    {
        return (await _artifactsClient.ListFeedViewsAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a feed view.")]
    public async Task<bool> DeleteFeedViewAsync(Guid feedId, string viewId)
    {
        return (await _artifactsClient.DeleteFeedViewAsync(feedId, viewId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Sets upstreaming behavior for a package.")]
    public async Task<bool> SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior)
    {
        return (await _artifactsClient.SetUpstreamingBehaviorAsync(feedId, packageName, behavior)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets upstreaming behavior for a package.")]
    public async Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(Guid feedId, string packageName)
    {
        return (await _artifactsClient.GetUpstreamingBehaviorAsync(feedId, packageName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets feed retention policy.")]
    public async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(Guid feedId)
    {
        return (await _artifactsClient.GetRetentionPolicyAsync(feedId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Sets feed retention policy.")]
    public async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy)
    {
        return (await _artifactsClient.SetRetentionPolicyAsync(feedId, policy)).EnsureSuccess(_logger);
    }
}
