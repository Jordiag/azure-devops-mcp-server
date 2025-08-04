using System.IO;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
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
            await WorkItemsClient.AddCommentAsync(epicId, AzureDevOpsConfiguration.ProjectName, comment);
            WorkItemComments comments = await WorkItemsClient.GetCommentsAsync(epicId);
            Assert.Contains(comments.Comments, c => c.Text == comment);
        }

        [Fact]
        public async Task AttachAndDownload_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic Attachment");
            byte[] data = "Test attachment"u8.ToArray();
            string fileName = $"attach-{UtcStamp()}.txt";
            await File.WriteAllBytesAsync(fileName, data);
            Guid? attachmentId = await WorkItemsClient.AddAttachmentAsync(epicId, fileName);
            Assert.True(attachmentId.HasValue);
            Stream? downloadedStream = await WorkItemsClient.GetAttachmentAsync(AzureDevOpsConfiguration.ProjectName, attachmentId.Value);
            Assert.NotNull(downloadedStream);
            using MemoryStream memory = new MemoryStream();
            await downloadedStream!.CopyToAsync(memory);
            Assert.Equal(data, memory.ToArray());
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
            IReadOnlyList<WorkItemUpdate> history = await WorkItemsClient.GetHistoryAsync(epicId);
            Assert.NotEmpty(history);
        }

        [Fact]
        public async Task LinkManagement_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Parent Epic");
            int childEpicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Child Epic");
            await WorkItemsClient.AddLinkAsync(epicId, childEpicId, "System.LinkTypes.Hierarchy-Forward");
            WorkItem? epic = await WorkItemsClient.GetWorkItemAsync(epicId);
            Assert.Contains(epic!.Relations!, r => r.Url.Contains(childEpicId.ToString()));
        }

        [Fact]
        public async Task GetWorkItemType_SucceedsAsync()
        {
            WorkItemType? type = await WorkItemsClient.GetWorkItemTypeAsync(AzureDevOpsConfiguration.ProjectName, "Task");
            Assert.NotNull(type);
        }

        [Fact]
        public async Task GetQuery_SucceedsAsync()
        {
            QueryHierarchyItem? query = await WorkItemsClient.GetQueryAsync(AzureDevOpsConfiguration.ProjectName, "Shared Queries/Work Items/All Work Items");
            Assert.NotNull(query);
        }

        [Fact]
        public async Task GetQueryResultsById_SucceedsAsync()
        {
            QueryHierarchyItem? query = await WorkItemsClient.GetQueryAsync(AzureDevOpsConfiguration.ProjectName, "Shared Queries/Work Items/All Work Items");
            Assert.NotNull(query);
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            WorkItemQueryResult result = await WorkItemsClient.GetQueryResultsByIdAsync(query!.Id, teamContext);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetWorkItemCount_SucceedsAsync()
        {
            string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project";
            int count = await WorkItemsClient.GetWorkItemCountAsync(wiql);
            Assert.True(count >= 0);
        }
    }
}
