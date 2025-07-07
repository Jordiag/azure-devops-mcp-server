using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Artifacts;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Tests.Common;

namespace Dotnet.AzureDevOps.Artifacts.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzureDevOpsArtifactsIntegrationTests : IAsyncLifetime
    {
        private readonly ArtifactsClient _artifactsClient;
        private readonly List<Guid> _createdFeedIds = [];
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

        public DotnetAzureDevOpsArtifactsIntegrationTests()
        {
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();

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
