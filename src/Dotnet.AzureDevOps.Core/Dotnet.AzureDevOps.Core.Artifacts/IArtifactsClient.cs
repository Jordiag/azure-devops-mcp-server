using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;

namespace Dotnet.AzureDevOps.Core.Artifacts;

public interface IArtifactsClient
{
    Task<Guid> CreateFeedAsync(FeedCreateOptions feedCreateOptions, CancellationToken cancellationToken = default);

    Task UpdateFeedAsync(Guid feedId, FeedUpdateOptions feedUpdateOptions, CancellationToken cancellationToken = default);

    Task<Feed?> GetFeedAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feed>> ListFeedsAsync(CancellationToken cancellationToken = default);

    Task DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Package>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<FeedView> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default);

    Task SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default);

    Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default);

    Task<Package> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default);

    Task<Stream> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default);

    Task<FeedRetentionPolicy> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default);

    Task<FeedRetentionPolicy> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default);
}