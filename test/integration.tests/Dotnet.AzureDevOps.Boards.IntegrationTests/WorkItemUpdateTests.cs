using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class WorkItemUpdateTests : BoardsIntegrationTestBase
    {
        public WorkItemUpdateTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task UpdateEpic_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic to Update");

            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = "Epic Updated Title",
                Description = "Updated description",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            };

            int? updatedId = await WorkItemsClient.UpdateEpicAsync(epicId, options);
            Assert.True(updatedId.HasValue);
        }

        [Fact]
        public async Task UpdateFeature_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Feature Update");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Feature to Update");

            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = "Feature Updated Title",
                Description = "Feature now updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            };

            int? updatedId = await WorkItemsClient.UpdateFeatureAsync(featureId, options);
            Assert.True(updatedId.HasValue);
        }

        [Fact]
        public async Task UpdateUserStory_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Story Update");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Feature for Story Update");
            int storyId = await WorkItemTestHelper.CreateUserStoryAsync(WorkItemsClient, CreatedWorkItemIds, featureId, "Story to Update");

            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = "Story Updated Title",
                Description = "Story now updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            };

            int? updatedId = await WorkItemsClient.UpdateUserStoryAsync(storyId, options);
            Assert.True(updatedId.HasValue);
        }

        [Fact]
        public async Task UpdateTask_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Task Update");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Feature for Task Update");
            int storyId = await WorkItemTestHelper.CreateUserStoryAsync(WorkItemsClient, CreatedWorkItemIds, featureId, "Story for Task Update");
            int taskId = await WorkItemTestHelper.CreateTaskAsync(WorkItemsClient, CreatedWorkItemIds, storyId, "Task to Update");

            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = "Task Updated Title",
                Description = "Task now updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            };

            int? updatedId = await WorkItemsClient.UpdateTaskAsync(taskId, options);
            Assert.True(updatedId.HasValue);
        }

        [Fact]
        public async Task UpdateWorkItem_SucceedsAsync()
        {
            int taskId = await WorkItemTestHelper.CreateTaskAsync(
                WorkItemsClient,
                CreatedWorkItemIds,
                await WorkItemTestHelper.CreateUserStoryAsync(
                    WorkItemsClient,
                    CreatedWorkItemIds,
                    await WorkItemTestHelper.CreateFeatureAsync(
                        WorkItemsClient,
                        CreatedWorkItemIds,
                        await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Generic Update"),
                        "Feature for Generic Update"),
                    "Story for Generic Update"),
                "Task for Generic Update");

            IList<WorkItemFieldUpdate> updates = new List<WorkItemFieldUpdate>
            {
                new WorkItemFieldUpdate { FieldReferenceName = "System.Title", Value = "Generic Task Updated" },
                new WorkItemFieldUpdate { FieldReferenceName = "System.Description", Value = "Updated via generic API" },
                new WorkItemFieldUpdate { FieldReferenceName = "System.State", Value = "Active" },
                new WorkItemFieldUpdate { FieldReferenceName = "System.Tags", Value = "IntegrationTest;Updated" }
            };

            WorkItem? updated = await WorkItemsClient.UpdateWorkItemAsync(taskId, updates);
            Assert.NotNull(updated);
        }
    }
}
