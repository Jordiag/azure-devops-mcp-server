using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Artifacts.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Artifacts)]
    public class DotnetAzureDevOpsArtifactsIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly ArtifactsClient _artifactsClient;
        private readonly List<Guid> _createdFeedIds = new List<Guid>();

        public DotnetAzureDevOpsArtifactsIntegrationTests(IntegrationTestFixture fixture)
        {
            _artifactsClient = fixture.ArtifactsClient;
        }

        [Fact]
        public async Task FeedCrud_SucceedsAsync()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"it-feed-{UtcStamp()}",
                Description = "Created by integration test"
            };

            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid id = createResult.Value;
            _createdFeedIds.Add(id);

            AzureDevOpsActionResult<Feed> getResult = await _artifactsClient.GetFeedAsync(id);
            Assert.True(getResult.IsSuccessful);
            Feed feed = getResult.Value;
            Assert.Equal(feedCreateOptions.Name, feed.Name);

            var updateOptions = new FeedUpdateOptions
            {
                Description = "Updated via test"
            };
            AzureDevOpsActionResult<bool> updateResult = await _artifactsClient.UpdateFeedAsync(id, updateOptions);
            Assert.True(updateResult.IsSuccessful);

            await WaitHelper.WaitUntilAsync(async () =>
            {
                getResult = await _artifactsClient.GetFeedAsync(id);
                return getResult.IsSuccessful && getResult.Value.Description == updateOptions.Description;
            }, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

            getResult = await _artifactsClient.GetFeedAsync(id);
            Assert.True(getResult.IsSuccessful);
            Assert.Equal("Updated via test", getResult.Value.Description);

            AzureDevOpsActionResult<IReadOnlyList<Feed>> listResult = await _artifactsClient.ListFeedsAsync();
            Assert.True(listResult.IsSuccessful);
            Assert.Contains(listResult.Value, f => f.Id == id);

            AzureDevOpsActionResult<bool> deleteResult = await _artifactsClient.DeleteFeedAsync(id);
            Assert.True(deleteResult.IsSuccessful);
            _createdFeedIds.Remove(id);

            AzureDevOpsActionResult<Feed> deletedResult = await _artifactsClient.GetFeedAsync(id);
            Assert.False(deletedResult.IsSuccessful);
        }

        [Fact]
        public async Task ListPackages_Empty_ForNewFeed()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"pkg-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid id = createResult.Value;
            _createdFeedIds.Add(id);

            AzureDevOpsActionResult<IReadOnlyList<Package>> packagesResult = await _artifactsClient.ListPackagesAsync(id);
            Assert.True(packagesResult.IsSuccessful);
            Assert.Empty(packagesResult.Value);
        }

        [Fact]
        public async Task FeedGetPermissions_SucceedsAsync()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"perm-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            AzureDevOpsActionResult<IReadOnlyList<FeedPermission>> permissionsResult = await _artifactsClient.GetFeedPermissionsAsync(feedId);
            Assert.True(permissionsResult.IsSuccessful);
            Assert.NotEmpty(permissionsResult.Value);
        }

        [Fact]
        public async Task FeedViewsWorkflow_SucceedsAsync()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"view-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            var view = new FeedView
            {
                Name = $"view-{UtcStamp()}",
                Visibility = "private",
                Type = "release"
            };

            AzureDevOpsActionResult<FeedView> createViewResult = await _artifactsClient.CreateFeedViewAsync(feedId, view);
            Assert.True(createViewResult.IsSuccessful);
            FeedView created = createViewResult.Value;

            AzureDevOpsActionResult<IReadOnlyList<FeedView>> listViewsResult = await _artifactsClient.ListFeedViewsAsync(feedId);
            Assert.True(listViewsResult.IsSuccessful);
            Assert.Contains(listViewsResult.Value, v => v.Id == created.Id);

            AzureDevOpsActionResult<bool> deleteViewResult = await _artifactsClient.DeleteFeedViewAsync(feedId, created.Id);
            Assert.True(deleteViewResult.IsSuccessful);

            await WaitHelper.WaitUntilAsync(async () =>
            {
                listViewsResult = await _artifactsClient.ListFeedViewsAsync(feedId);
                return listViewsResult.IsSuccessful && !listViewsResult.Value.Any(v => v.Id == created.Id);
            }, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

            Assert.True(listViewsResult.IsSuccessful);
            Assert.DoesNotContain(listViewsResult.Value, v => v.Id == created.Id);
        }

        [Fact]
        public async Task RetentionPolicyWorkflow_SucceedsAsync()
        {
            int days = 30;
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"ret-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            var create = new FeedRetentionPolicy
            {
                AgeLimitInDays = days - 1,
                CountLimit = days - 1,
                DaysToKeepRecentlyDownloadedPackages = days - 1
            };

            AzureDevOpsActionResult<FeedRetentionPolicy> feedRetentionPolicyResult = await _artifactsClient.SetRetentionPolicyAsync(feedId, create);
            Assert.True(feedRetentionPolicyResult.IsSuccessful);

            var update = new FeedRetentionPolicy
            {
                AgeLimitInDays = 3000,
                CountLimit = create.CountLimit,
                DaysToKeepRecentlyDownloadedPackages = days
            };

            AzureDevOpsActionResult<FeedRetentionPolicy> updateResult = await _artifactsClient.SetRetentionPolicyAsync(feedId, update);
            Assert.True(updateResult.IsSuccessful);

            AzureDevOpsActionResult<FeedRetentionPolicy> policyResult = await _artifactsClient.GetRetentionPolicyAsync(feedId);

            Assert.True(policyResult.IsSuccessful);
            FeedRetentionPolicy updatedPolicy = policyResult.Value;
            Assert.Null(updatedPolicy.AgeLimitInDays);
            Assert.Equal(updatedPolicy.CountLimit, create.CountLimit);
            Assert.Equal(updatedPolicy.DaysToKeepRecentlyDownloadedPackages, days);
        }

        [Fact]
        public async Task PackageAndUpstreaming_Methods_ReturnNotFoundAsync()
        {
            var feedCreateOptions = new FeedCreateOptions
            {
                Name = $"pkgerr-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            const string packageName = "non-existent";
            const string version = "1.0.0";

            AzureDevOpsActionResult<bool> deletePackageResult = await _artifactsClient.DeletePackageAsync(feedId, packageName, version);
            Assert.False(deletePackageResult.IsSuccessful);

            AzureDevOpsActionResult<Package> getPackageResult = await _artifactsClient.GetPackageVersionAsync(feedId, packageName, version);
            Assert.False(getPackageResult.IsSuccessful);

            AzureDevOpsActionResult<bool> updatePackageResult = await _artifactsClient.UpdatePackageVersionAsync(feedId, packageName, version, new PackageVersionDetails());
            Assert.False(updatePackageResult.IsSuccessful);

            AzureDevOpsActionResult<Stream> downloadResult = await _artifactsClient.DownloadPackageAsync(feedId, packageName, version);
            Assert.False(downloadResult.IsSuccessful);

            AzureDevOpsActionResult<bool> setUpstreamResult = await _artifactsClient.SetUpstreamingBehaviorAsync(feedId, packageName, UpstreamingBehavior.Block);
            Assert.False(setUpstreamResult.IsSuccessful);

            AzureDevOpsActionResult<UpstreamingBehavior> getUpstreamResult = await _artifactsClient.GetUpstreamingBehaviorAsync(feedId, packageName);
            Assert.False(getUpstreamResult.IsSuccessful);
        }

        /*────────── IAsyncLifetime ──────────*/
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(Guid id in _createdFeedIds.AsEnumerable().Reverse())
            {
                try
                {
                    AzureDevOpsActionResult<bool> result = await _artifactsClient.DeleteFeedAsync(id);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        private static string UtcStamp() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
