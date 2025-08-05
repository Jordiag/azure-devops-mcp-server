using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Xunit;

namespace Dotnet.AzureDevOps.Artifacts.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Artifacts)]
    public class DotnetAzureDevOpsArtifactsIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly ArtifactsClient _artifactsClient;
        private readonly List<Guid> _createdFeedIds = new List<Guid>();
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

        public DotnetAzureDevOpsArtifactsIntegrationTests(IntegrationTestFixture fixture)
        {
            _azureDevOpsConfiguration = fixture.Configuration;
            _artifactsClient = fixture.ArtifactsClient;
        }

        [Fact]
        public async Task FeedCrud_SucceedsAsync()
        {
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
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

            FeedUpdateOptions updateOptions = new FeedUpdateOptions
            {
                Description = "Updated via test"
            };
            AzureDevOpsActionResult<bool> updateResult = await _artifactsClient.UpdateFeedAsync(id, updateOptions);
            Assert.True(updateResult.IsSuccessful);

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
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
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
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
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
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
            {
                Name = $"view-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            FeedView view = new FeedView
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

            listViewsResult = await _artifactsClient.ListFeedViewsAsync(feedId);
            Assert.True(listViewsResult.IsSuccessful);
            Assert.DoesNotContain(listViewsResult.Value, v => v.Id == created.Id);
        }

        [Fact]
        public async Task RetentionPolicyWorkflow_SucceedsAsync()
        {
            int days = 30;
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
            {
                Name = $"ret-feed-{UtcStamp()}"
            };
            AzureDevOpsActionResult<Guid> createResult = await _artifactsClient.CreateFeedAsync(feedCreateOptions);
            Assert.True(createResult.IsSuccessful);
            Guid feedId = createResult.Value;
            _createdFeedIds.Add(feedId);

            AzureDevOpsActionResult<FeedRetentionPolicy> policyResult = await _artifactsClient.GetRetentionPolicyAsync(feedId);
            Assert.True(policyResult.IsSuccessful);
            FeedRetentionPolicy policy = policyResult.Value;
            FeedRetentionPolicy update = new FeedRetentionPolicy
            {
                AgeLimitInDays = policy.AgeLimitInDays ?? days,
                CountLimit = policy.CountLimit ?? days,
                DaysToKeepRecentlyDownloadedPackages = policy.DaysToKeepRecentlyDownloadedPackages ?? days
            };

            AzureDevOpsActionResult<FeedRetentionPolicy> updateResult = await _artifactsClient.SetRetentionPolicyAsync(feedId, update);
            Assert.True(updateResult.IsSuccessful);

            policyResult = await _artifactsClient.GetRetentionPolicyAsync(feedId);
            Assert.True(policyResult.IsSuccessful);
            FeedRetentionPolicy updatedPolicy = policyResult.Value;
            Assert.Null(updatedPolicy.AgeLimitInDays);
            Assert.Equal(days, updatedPolicy.CountLimit);
            Assert.Equal(days, updatedPolicy.DaysToKeepRecentlyDownloadedPackages);
        }

        [Fact]
        public async Task PackageAndUpstreaming_Methods_ReturnNotFoundAsync()
        {
            FeedCreateOptions feedCreateOptions = new FeedCreateOptions
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
                }
            }
        }

        private static string UtcStamp() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
