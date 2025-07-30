using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Artifacts.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Artifacts)]
    public class DotnetAzureDevOpsArtifactsIntegrationTests : IAsyncLifetime
    {
        private readonly ArtifactsClient _artifactsClient;
        private readonly List<Guid> _createdFeedIds = [];
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

        public DotnetAzureDevOpsArtifactsIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

            _artifactsClient = new ArtifactsClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        [Fact]
        public async Task FeedCrud_SucceedsAsync()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"it-feed-{UtcStamp()}",
                Description = "Created by integration test"
            };

            Guid id = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            _createdFeedIds.Add(id);

            Feed? feed = await _artifactsClient.GetFeedAsync(id);
            Assert.NotNull(feed);
            Assert.Equal(feedCreateOptions.Name, feed!.Name);

            await _artifactsClient.UpdateFeedAsync(id, new FeedUpdateOptions
            {
                Description = "Updated via test"
            });

            feed = await _artifactsClient.GetFeedAsync(id);
            Assert.Equal("Updated via test", feed!.Description);

            IReadOnlyList<Feed> list = await _artifactsClient.ListFeedsAsync();
            Assert.Contains(list, f => f.Id == id);

            await _artifactsClient.DeleteFeedAsync(id);
            _createdFeedIds.Remove(id);

            Feed? deleted = await _artifactsClient.GetFeedAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task ListPackages_Empty_ForNewFeed()
        {
            Guid id = await _artifactsClient.CreateFeedAsync(new FeedCreateOptions
            {
                Name = $"pkg-feed-{UtcStamp()}"
            });
            _createdFeedIds.Add(id);

            IReadOnlyList<Package> packages = await _artifactsClient.ListPackagesAsync(id);
            Assert.Empty(packages);
        }

        [Fact]
        public async Task FeedGetPermissions_SucceedsAsync()
        {
            Guid feedId = await _artifactsClient.CreateFeedAsync(new FeedCreateOptions
            {
                Name = $"perm-feed-{UtcStamp()}"
            });
            _createdFeedIds.Add(feedId);

            IReadOnlyList<FeedPermission> permissions = await _artifactsClient.GetFeedPermissionsAsync(feedId);
            Assert.NotNull(permissions);
        }

        [Fact]
        public async Task FeedViewsWorkflow_SucceedsAsync()
        {
            Guid feedId = await _artifactsClient.CreateFeedAsync(new FeedCreateOptions
            {
                Name = $"view-feed-{UtcStamp()}"
            });
            _createdFeedIds.Add(feedId);

            var view = new FeedView
            {
                Name = $"view-{UtcStamp()}",
                Visibility = "private",
                Type = "release"
            };

            FeedView created = await _artifactsClient.CreateFeedViewAsync(feedId, view);
            IReadOnlyList<FeedView> views = await _artifactsClient.ListFeedViewsAsync(feedId);
            Assert.Contains(views, v => v.Id == created.Id);

            await _artifactsClient.DeleteFeedViewAsync(feedId, created.Id);

            IReadOnlyList<FeedView> afterDelete = await _artifactsClient.ListFeedViewsAsync(feedId);
            Assert.DoesNotContain(afterDelete, v => v.Id == created.Id);
        }

        [Fact]
        public async Task RetentionPolicyWorkflow_SucceedsAsync()
        {
            int days = 30;
            Guid feedId = await _artifactsClient.CreateFeedAsync(new FeedCreateOptions
            {
                Name = $"ret-feed-{UtcStamp()}"
            });
            _createdFeedIds.Add(feedId);

            FeedRetentionPolicy policy = await _artifactsClient.GetRetentionPolicyAsync(feedId);
            var update = new FeedRetentionPolicy
            {
                AgeLimitInDays = policy?.AgeLimitInDays ?? days,
                CountLimit = policy?.CountLimit ?? days,
                DaysToKeepRecentlyDownloadedPackages = policy?.DaysToKeepRecentlyDownloadedPackages ?? days
            };

            FeedRetentionPolicy updated = await _artifactsClient.SetRetentionPolicyAsync(feedId, update);
            Assert.NotNull(updated);

            policy = await _artifactsClient.GetRetentionPolicyAsync(feedId);
            Assert.Null(policy.AgeLimitInDays);
            Assert.Equal(days, policy.CountLimit);
            Assert.Equal(days, policy.DaysToKeepRecentlyDownloadedPackages);
        }

        [Fact]
        public async Task PackageAndUpstreaming_Methods_ReturnNotFoundAsync()
        {
            Guid feedId = await _artifactsClient.CreateFeedAsync(new FeedCreateOptions
            {
                Name = $"pkgerr-feed-{UtcStamp()}"
            });
            _createdFeedIds.Add(feedId);

            const string packageName = "non-existent";
            const string version = "1.0.0";

            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.DeletePackageAsync(feedId, packageName, version));
            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.GetPackageVersionAsync(feedId, packageName, version));
            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.UpdatePackageVersionAsync(feedId, packageName, version, new PackageVersionDetails()));
            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.DownloadPackageAsync(feedId, packageName, version));
            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.SetUpstreamingBehaviorAsync(feedId, packageName, UpstreamingBehavior.Block));
            await Assert.ThrowsAsync<HttpRequestException>(() => _artifactsClient.GetUpstreamingBehaviorAsync(feedId, packageName));
        }

        /*────────── IAsyncLifetime ──────────*/
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(Guid id in _createdFeedIds.AsEnumerable().Reverse())
            {
                try
                {
                    await _artifactsClient.DeleteFeedAsync(id);
                }
                catch
                {
                    // Ignore errors during cleanup, as feeds may have been deleted already  
                    // or there could be other issues that prevent deletion.  
                }
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
