using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class BatchOperationTests : BoardsIntegrationTestBase
    {
        public BatchOperationTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task ExecuteBatch_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Batch Execute Epic");

            WitBatchRequest request = new WitBatchRequest
            {
                Method = "GET",
                Uri = $"/_apis/wit/workitems/{epicId}?api-version={GlobalConstants.ApiVersion}"
            };

            IReadOnlyList<WitBatchResponse> responses = await WorkItemsClient.ExecuteBatchAsync(new List<WitBatchRequest> { request });
            Assert.Single(responses);
        }

        [Fact]
        public async Task CreateWorkItemsMultipleCalls_SucceedsAsync()
        {
            string title = $"batch-{UtcStamp()}";
            IList<WorkItemCreateOptions> items = new List<WorkItemCreateOptions>
            {
                new WorkItemCreateOptions { Title = title, Description = "Batch epic", Tags = "IntegrationTest" },
                new WorkItemCreateOptions { Title = $"{title}-2", Description = "Batch epic 2", Tags = "IntegrationTest" }
            };

            IReadOnlyList<int> identifiers = await WorkItemsClient.CreateWorkItemsMultipleCallsAsync("Epic", items);
            Assert.Equal(2, identifiers.Count);
            foreach(int identifier in identifiers)
            {
                CreatedWorkItemIds.Add(identifier);
            }
        }

        [Fact]
        public async Task UpdateWorkItemsBatch_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Batch Update Epic");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Batch Update Feature");

            IList<(int Id, WorkItemCreateOptions Options)> updates = new List<(int, WorkItemCreateOptions)>
            {
                (epicId, new WorkItemCreateOptions { Title = "Updated Epic" }),
                (featureId, new WorkItemCreateOptions { Title = "Updated Feature" })
            };

            IReadOnlyList<WitBatchResponse> updated = await WorkItemsClient.UpdateWorkItemsBatchAsync(updates);
            Assert.Equal(2, updated.Count);
        }

        [Fact]
        public async Task LinkWorkItemsBatch_SucceedsAsync()
        {
            int firstEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Link Batch Epic 1");
            int secondEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Link Batch Epic 2");

            IList<(int SourceId, int TargetId, string Relation)> links = new List<(int, int, string)>
            {
                (firstEpicId, secondEpicId, "System.LinkTypes.Related")
            };

            IReadOnlyList<WitBatchResponse> response = await WorkItemsClient.LinkWorkItemsBatchAsync(links);
            Assert.Single(response);
        }

        [Fact]
        public async Task CloseWorkItemsBatch_SucceedsAsync()
        {
            int firstEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Close Batch Epic 1");
            int secondEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Close Batch Epic 2");

            IReadOnlyList<WitBatchResponse> closed = await WorkItemsClient.CloseWorkItemsBatchAsync(new List<int> { firstEpicId, secondEpicId });
            Assert.Equal(2, closed.Count);
        }

        [Fact]
        public async Task CloseAndLinkDuplicatesBatch_SucceedsAsync()
        {
            int canonicalId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Original Epic");
            int duplicateId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Duplicate Epic");

            IList<(int DuplicateId, int CanonicalId)> pairs = new List<(int, int)>
            {
                (duplicateId, canonicalId)
            };

            IReadOnlyList<WitBatchResponse> closed = await WorkItemsClient.CloseAndLinkDuplicatesBatchAsync(pairs);
            Assert.Single(closed);
        }

        [Fact]
        public async Task GetWorkItemsBatchByIds_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Batch Get Epic");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Batch Get Feature");
            IReadOnlyList<WorkItem?> items = await WorkItemsClient.GetWorkItemsBatchByIdsAsync(new List<int> { epicId, featureId });
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task LinkWorkItemsByNameBatch_SucceedsAsync()
        {
            int? firstId = await WorkItemsClient.CreateEpicAsync(new WorkItemCreateOptions { Title = "Batch Name Epic 1", Description = "test", Tags = "IntegrationTest" });
            int? secondId = await WorkItemsClient.CreateEpicAsync(new WorkItemCreateOptions { Title = "Batch Name Epic 2", Description = "test", Tags = "IntegrationTest" });
            Assert.True(firstId.HasValue && secondId.HasValue);
            CreatedWorkItemIds.Add(firstId!.Value);
            CreatedWorkItemIds.Add(secondId!.Value);

            IList<(int SourceId, int TargetId, string Type, string? Comment)> links = new List<(int, int, string, string?)>
            {
                (firstId.Value, secondId.Value, "System.LinkTypes.Related", "link")
            };

            IReadOnlyList<WitBatchResponse> response = await WorkItemsClient.LinkWorkItemsByNameBatchAsync(links);
            Assert.Single(response);
        }
    }
}
