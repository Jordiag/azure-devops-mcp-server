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
}
