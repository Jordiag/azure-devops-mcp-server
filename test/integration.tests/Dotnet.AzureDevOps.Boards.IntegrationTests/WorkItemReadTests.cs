using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class WorkItemReadTests : BoardsIntegrationTestBase
    {
        public WorkItemReadTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task ReadEpic_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic to Read");
            WorkItem? epic = await WorkItemsClient.GetWorkItemAsync(epicId);
            Assert.NotNull(epic);
        }

        [Fact]
        public async Task QueryWorkItems_SucceedsAsync()
        {
            string query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project";
            IList<WorkItemReference> results = await WorkItemsClient.QueryWorkItemsAsync(query);
            Assert.NotNull(results);
        }

        [Fact]
        public async Task AddAndReadComments_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic Comments");
            string comment = "This is a test comment";
            int? commentId = await WorkItemsClient.AddCommentAsync(epicId, comment);
            Assert.True(commentId.HasValue);
            WorkItemComments comments = await WorkItemsClient.GetCommentsAsync(epicId);
            Assert.Contains(comments.Comments, c => c.Text == comment);
        }

        [Fact]
        public async Task AttachAndDownload_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic Attachment");
            byte[] data = "Test attachment"u8.ToArray();
            string fileName = $"attach-{UtcStamp()}.txt";
            int? attachmentId = await WorkItemsClient.AttachFileAsync(epicId, data, fileName);
            Assert.True(attachmentId.HasValue);
            byte[] downloaded = await WorkItemsClient.DownloadAttachmentAsync(attachmentId.Value);
            Assert.Equal(data, downloaded);
        }

        [Fact]
        public async Task WorkItemHistory_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic History");
            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = "Epic History Updated",
                Description = "Updated for history",
                Tags = "IntegrationTest"
            };
            await WorkItemsClient.UpdateEpicAsync(epicId, options);
            IList<WorkItem> history = await WorkItemsClient.GetWorkItemHistoryAsync(epicId);
            Assert.NotEmpty(history);
        }

        [Fact]
        public async Task LinkManagement_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Parent Epic");
            int childEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Child Epic");
            await WorkItemsClient.LinkWorkItemsAsync(epicId, childEpicId);
            WorkItem? epic = await WorkItemsClient.GetWorkItemAsync(epicId);
            Assert.Contains(epic!.Relations!, r => r.Url.Contains(childEpicId.ToString()));
        }

        [Fact]
        public async Task BulkEdit_SucceedsAsync()
        {
            (int EpicId, int FeatureId, int StoryId, int TaskId) hierarchy = await WorkItemTestHelper.CreateWorkItemHierarchyAsync(WorkItemsClient, CreatedWorkItemIds);
            var batch = new List<WorkItemBatchOptions>
            {
                new WorkItemBatchOptions
                {
                    Id = hierarchy.EpicId,
                    Fields = new Dictionary<string, object> { { "System.Title", "Bulk Edited Epic" } }
                },
                new WorkItemBatchOptions
                {
                    Id = hierarchy.FeatureId,
                    Fields = new Dictionary<string, object> { { "System.Title", "Bulk Edited Feature" } }
                }
            };

            IReadOnlyList<WorkItem?> edited = await WorkItemsClient.BulkEditAsync(batch);
            Assert.Equal(2, edited.Count);
        }

        [Fact]
        public async Task GetWorkItemType_SucceedsAsync()
        {
            WorkItemType? type = await WorkItemsClient.GetWorkItemTypeAsync("Task");
            Assert.NotNull(type);
        }

        [Fact]
        public async Task GetQuery_SucceedsAsync()
        {
            QueryHierarchyItem? query = await WorkItemsClient.GetQueryAsync("Shared Queries", "Work Items", "All Work Items");
            Assert.NotNull(query);
        }

        [Fact]
        public async Task GetQueryResultsById_SucceedsAsync()
        {
            QueryHierarchyItem? query = await WorkItemsClient.GetQueryAsync("Shared Queries", "Work Items", "All Work Items");
            Assert.NotNull(query);
            IList<WorkItemReference> result = await WorkItemsClient.GetQueryResultsByIdAsync(query!.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetWorkItemCount_SucceedsAsync()
        {
            int count = await WorkItemsClient.GetWorkItemCountAsync();
            Assert.True(count >= 0);
        }
    }
}
