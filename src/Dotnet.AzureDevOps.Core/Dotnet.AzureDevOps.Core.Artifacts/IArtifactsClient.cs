using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;

namespace Dotnet.AzureDevOps.Core.Artifacts;

public interface IArtifactsClient
{
    Task<AzureDevOpsActionResult<Guid>> CreateFeedAsync(FeedCreateOptions feedCreateOptions, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> UpdateFeedAsync(Guid feedId, FeedUpdateOptions feedUpdateOptions, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<Feed>> GetFeedAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<Feed>>> ListFeedsAsync(CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<Package>>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<FeedView>> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<FeedView>>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<UpstreamingBehavior>> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<Package>> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<bool>> UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<Stream>> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<FeedRetentionPolicy>> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<FeedRetentionPolicy>> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default);
}
