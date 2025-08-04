using Dotnet.AzureDevOps.Core.Boards.Options;
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
            (int EpicId, int FeatureId, int StoryId, int TaskId) hierarchy = await WorkItemTestHelper.CreateWorkItemHierarchyAsync(WorkItemsClient, CreatedWorkItemIds);
            IReadOnlyList<WorkItem?> items = await WorkItemsClient.GetWorkItemsBatchByIdsAsync(new List<int> { hierarchy.EpicId, hierarchy.FeatureId });
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateWorkItemsBatch_SucceedsAsync()
        {
            string title = $"batch-{UtcStamp()}";
            IList<WorkItemCreateOptions> batch = new List<WorkItemCreateOptions>
            {
                new WorkItemCreateOptions { Title = title, Description = "Batch epic", Tags = "IntegrationTest", WorkItemType = "Epic" },
                new WorkItemCreateOptions { Title = title, Description = "Batch feature", Tags = "IntegrationTest", WorkItemType = "Feature" }
            };

            IReadOnlyList<int> ids = await WorkItemsClient.CreateWorkItemsBatchAsync(batch);
            Assert.Equal(2, ids.Count);
            foreach(int id in ids)
            {
                CreatedWorkItemIds.Add(id);
            }
        }

        [Fact]
        public async Task UpdateWorkItemsBatch_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Batch Update Epic");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Batch Update Feature");

            IList<WorkItemBatchOptions> batch = new List<WorkItemBatchOptions>
            {
                new WorkItemBatchOptions { Id = epicId, Fields = new Dictionary<string, object> { { "System.Title", "Updated Epic" } } },
                new WorkItemBatchOptions { Id = featureId, Fields = new Dictionary<string, object> { { "System.Title", "Updated Feature" } } }
            };

            IReadOnlyList<WorkItem?> updated = await WorkItemsClient.UpdateWorkItemsBatchAsync(batch);
            Assert.Equal(2, updated.Count);
        }

        [Fact]
        public async Task LinkWorkItemsBatch_SucceedsAsync()
        {
            int w1 = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Link Batch Epic 1");
            int w2 = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Link Batch Epic 2");

            IList<WorkItemLinkBatchOptions> links = new List<WorkItemLinkBatchOptions>
            {
                new WorkItemLinkBatchOptions { SourceId = w1, TargetId = w2, LinkType = "related" }
            };

            IReadOnlyList<WitBatchResponse> response = await WorkItemsClient.LinkWorkItemsBatchAsync(links);
            Assert.Single(response);
        }

        [Fact]
        public async Task CloseWorkItemsBatch_SucceedsAsync()
        {
            int w1 = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Close Batch Epic 1");
            int w2 = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Close Batch Epic 2");

            IList<WorkItemCloseBatchOptions> closeOptions = new List<WorkItemCloseBatchOptions>
            {
                new WorkItemCloseBatchOptions { Id = w1, Comment = "closing" },
                new WorkItemCloseBatchOptions { Id = w2, Comment = "closing" }
            };

            IReadOnlyList<WorkItem?> closed = await WorkItemsClient.CloseWorkItemsBatchAsync(closeOptions);
            Assert.Equal(2, closed.Count);
        }

        [Fact]
        public async Task CloseAndLinkDuplicatesBatch_SucceedsAsync()
        {
            int original = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Original Epic");
            int duplicate = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Duplicate Epic");

            IList<WorkItemCloseBatchOptions> closeOptions = new List<WorkItemCloseBatchOptions>
            {
                new WorkItemCloseBatchOptions { Id = original, Comment = "closing" },
                new WorkItemCloseBatchOptions { Id = duplicate, Comment = "closing", DuplicateOf = original }
            };

            IReadOnlyList<WorkItem?> closed = await WorkItemsClient.CloseWorkItemsBatchAsync(closeOptions);
            Assert.Equal(2, closed.Count);
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
            int? w1 = await WorkItemsClient.CreateEpicAsync(new WorkItemCreateOptions { Title = "Batch Name Epic 1", Description = "test", Tags = "IntegrationTest" });
            int? w2 = await WorkItemsClient.CreateEpicAsync(new WorkItemCreateOptions { Title = "Batch Name Epic 2", Description = "test", Tags = "IntegrationTest" });
            Assert.True(w1.HasValue && w2.HasValue);
            CreatedWorkItemIds.Add(w1!.Value);
            CreatedWorkItemIds.Add(w2!.Value);

            IList<(int SourceId, int TargetId, string LinkType, string Relation)> links = new List<(int, int, string, string)>
            {
                (w1.Value, w2.Value, "related", "link")
            };

            IReadOnlyList<WitBatchResponse> resp = await WorkItemsClient.LinkWorkItemsByNameBatchAsync(links);
            Assert.Single(resp);
        }
    }
}
