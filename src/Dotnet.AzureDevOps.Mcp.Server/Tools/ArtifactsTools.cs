using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
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
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Guid> result = await client.CreateFeedAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create feed.");
        return result.Value;
    }

    [McpServerTool, Description("Updates an existing feed.")]
    public static async Task UpdateFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedUpdateOptions options)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.UpdateFeedAsync(feedId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to update feed.");
    }

    [McpServerTool, Description("Retrieves a feed by identifier.")]
    public static async Task<Feed?> GetFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Feed> result = await client.GetFeedAsync(feedId);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Lists all feeds in the project.")]
    public static async Task<IReadOnlyList<Feed>> ListFeedsAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<Feed>> result = await client.ListFeedsAsync();
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list feeds.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a feed by identifier.")]
    public static async Task DeleteFeedAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteFeedAsync(feedId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete feed.");
    }

    [McpServerTool, Description("Lists packages within a feed.")]
    public static async Task<IReadOnlyList<Package>> ListPackagesAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<Package>> result = await client.ListPackagesAsync(feedId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list packages.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a package from a feed.")]
    public static async Task DeletePackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeletePackageAsync(feedId, packageName, version);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete package.");
    }

    [McpServerTool, Description("Gets permissions for a feed.")]
    public static async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<FeedPermission>> result = await client.GetFeedPermissionsAsync(feedId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get feed permissions.");
        return result.Value;
    }

    [McpServerTool, Description("Creates a feed view.")]
    public static async Task<FeedView> CreateFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedView view)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<FeedView> result = await client.CreateFeedViewAsync(feedId, view);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create feed view.");
        return result.Value;
    }

    [McpServerTool, Description("Lists feed views.")]
    public static async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<FeedView>> result = await client.ListFeedViewsAsync(feedId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list feed views.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a feed view.")]
    public static async Task DeleteFeedViewAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string viewId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteFeedViewAsync(feedId, viewId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete feed view.");
    }

    [McpServerTool, Description("Sets upstreaming behavior for a package.")]
    public static async Task SetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, UpstreamingBehavior behavior)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.SetUpstreamingBehaviorAsync(feedId, packageName, behavior);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to set upstreaming behavior.");
    }

    [McpServerTool, Description("Gets upstreaming behavior for a package.")]
    public static async Task<UpstreamingBehavior?> GetUpstreamingBehaviorAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<UpstreamingBehavior> result = await client.GetUpstreamingBehaviorAsync(feedId, packageName);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Gets a specific package version.")]
    public static async Task<Package?> GetPackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Package> result = await client.GetPackageVersionAsync(feedId, packageName, version);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Updates metadata for a package version.")]
    public static async Task UpdatePackageVersionAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version, PackageVersionDetails details)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.UpdatePackageVersionAsync(feedId, packageName, version, details);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to update package version.");
    }

    [McpServerTool, Description("Downloads a package version.")]
    public static async Task<Stream> DownloadPackageAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, string packageName, string version)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Stream> result = await client.DownloadPackageAsync(feedId, packageName, version);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to download package.");
        return result.Value;
    }

    [McpServerTool, Description("Gets the retention policy for a feed.")]
    public static async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<FeedRetentionPolicy> result = await client.GetRetentionPolicyAsync(feedId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get retention policy.");
        return result.Value;
    }

    [McpServerTool, Description("Sets the retention policy for a feed.")]
    public static async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(string organizationUrl, string projectName, string personalAccessToken, Guid feedId, FeedRetentionPolicy policy)
    {
        ArtifactsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<FeedRetentionPolicy> result = await client.SetRetentionPolicyAsync(feedId, policy);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to set retention policy.");
        return result.Value;
    }
}
