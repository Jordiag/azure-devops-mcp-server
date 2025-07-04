using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzureDevOpsBoardsIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WorkItemsClient _workItemsClient;
        private readonly List<int> _createdWorkItemIds = [];

        public DotnetAzureDevOpsBoardsIntegrationTests()
        {
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();

            _workItemsClient = new WorkItemsClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        /// <summary>
        /// Test creating an Epic on its own.
        /// </summary>
        [Fact]
        public async Task CreateEpic_SucceedsAsync()
        {
            // Arrange
            var options = new WorkItemCreateOptions
            {
                Title = "Integration Test Epic",
                Description = "Epic created by xUnit test",
                Tags = "IntegrationTest"
            };

            // Act
            int? epicId = await _workItemsClient.CreateEpicAsync(options);

            // Assert
            Assert.True(epicId.HasValue, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);
        }

        /// <summary>
        /// Test creating a Feature that references a newly created Epic.
        /// </summary>
        [Fact]
        public async Task CreateFeature_SucceedsAsync()
        {
            // First, create an Epic so the Feature has a parent
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Feature Test",
                Description = "Parent epic for feature creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic (for Feature). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Now, create a Feature linked to that Epic
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Feature",
                Description = "Feature referencing epic",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });

            Assert.True(featureId.HasValue, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);
        }

        /// <summary>
        /// Test creating a User Story that references a newly created Feature (which references an Epic).
        /// </summary>
        [Fact]
        public async Task CreateUserStory_SucceedsAsync()
        {
            // Create an Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Story Test",
                Description = "Parent epic for story creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic (for Story). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Create a Feature (child of Epic)
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Story Test",
                Description = "Parent feature for story creation",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.HasValue, "Failed to create Feature (for Story). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // Create a User Story (child of Feature)
            int? storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Story",
                Description = "Story referencing feature",
                ParentId = featureId,
                Tags = "IntegrationTest"
            });

            Assert.True(storyId.HasValue, "Failed to create User Story. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);
        }

        /// <summary>
        /// Test creating a Task that references a newly created User Story (which references a Feature, which references an Epic).
        /// </summary>
        [Fact]
        public async Task CreateTask_SucceedsAsync()
        {
            // Create an Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Task Test",
                Description = "Parent epic for task creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic (for Task). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Create a Feature
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Task Test",
                Description = "Parent feature for task creation",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.HasValue, "Failed to create Feature (for Task). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // Create a User Story
            int? storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story for Task Test",
                Description = "Parent story for task creation",
                ParentId = featureId,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.HasValue, "Failed to create User Story (for Task). ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // Finally, create a Task (child of the Story)
            int? taskId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Task",
                Description = "Task referencing user story",
                ParentId = storyId,
                Tags = "IntegrationTest"
            });

            Assert.True(taskId.HasValue, "Failed to create Task. ID was null.");
            _createdWorkItemIds.Add(taskId.Value);
        }

        /// <summary>
        /// Update an Epic: create it first, then modify fields and confirm update succeeded.
        /// </summary>
        [Fact]
        public async Task UpdateEpic_SucceedsAsync()
        {
            // 1. Create an Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic to Update",
                Description = "Original description",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic for update test. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Update the Epic
            int? updatedId = await _workItemsClient.UpdateEpicAsync(epicId.Value, new WorkItemCreateOptions
            {
                Title = "Epic Updated Title",
                Description = "Updated description",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });

            Assert.True(updatedId.HasValue, "Failed to update Epic. ID was null.");
            // updatedId should be the same as epicId, but we only store epicId in the list once
        }

        /// <summary>
        /// Update a Feature: create a parent Epic and the Feature, then modify fields on the Feature.
        /// </summary>
        [Fact]
        public async Task UpdateFeature_SucceedsAsync()
        {
            // 1. Create an Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Feature Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create the Feature referencing that Epic
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature to Update",
                Description = "Original feature",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.HasValue, "Failed to create Feature (for update). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Update the Feature
            int? updatedId = await _workItemsClient.UpdateFeatureAsync(featureId.Value, new WorkItemCreateOptions
            {
                Title = "Feature Updated Title",
                Description = "Feature now updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.HasValue, "Failed to update Feature. ID was null.");
        }

        /// <summary>
        /// Update a User Story: create an Epic, Feature, then a Story, then update the Story's fields.
        /// </summary>
        [Fact]
        public async Task UpdateUserStory_SucceedsAsync()
        {
            // 1. Create Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Story Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create Feature
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Story Update",
                Description = "Parent feature",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.HasValue, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Create User Story
            int? storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story to Update",
                Description = "Original story description",
                ParentId = featureId,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.HasValue, "Failed to create Story for update test. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // 4. Update the Story
            int? updatedId = await _workItemsClient.UpdateUserStoryAsync(storyId.Value, new WorkItemCreateOptions
            {
                Title = "Story Updated Title",
                Description = "Story has been updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.HasValue, "Failed to update Story. ID was null.");
        }

        /// <summary>
        /// Update a Task: create the chain (Epic -> Feature -> Story -> Task),
        /// then modify fields on the Task.
        /// </summary>
        [Fact]
        public async Task UpdateTask_SucceedsAsync()
        {
            // 1. Create Epic
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Task Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.HasValue, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create Feature
            int? featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Task Update",
                Description = "Parent feature",
                ParentId = epicId,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.HasValue, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Create User Story
            int? storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story for Task Update",
                Description = "Parent story",
                ParentId = featureId,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.HasValue, "Failed to create Story. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // 4. Create Task
            int? taskId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions
            {
                Title = "Task to Update",
                Description = "Original task description",
                ParentId = storyId,
                Tags = "IntegrationTest"
            });
            Assert.True(taskId.HasValue, "Failed to create Task (for update). ID was null.");
            _createdWorkItemIds.Add(taskId.Value);

            // 5. Update the Task
            int? updatedId = await _workItemsClient.UpdateTaskAsync(taskId.Value, new WorkItemCreateOptions
            {
                Title = "Task Updated Title",
                Description = "Updated task description",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.HasValue, "Failed to update Task. ID was null.");
        }

        /// <summary>
        /// Creates an Epic, then reads it back to verify fields.
        /// </summary>
        [Fact]
        public async Task ReadEpic_SucceedsAsync()
        {
            // 1. Create an Epic
            var createOptions = new WorkItemCreateOptions
            {
                Title = "Read Epic Test",
                Description = "This epic is for read test",
                Tags = "IntegrationTest;Read"
            };

            int? epicId = await _workItemsClient.CreateEpicAsync(createOptions);
            Assert.True(epicId.HasValue, "Failed to create Epic for read test. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Read the newly created Epic
            WorkItem? epicWorkItem = await _workItemsClient.GetWorkItemAsync(epicId.Value);
            Assert.NotNull(epicWorkItem); // we expect it to exist

            // 3. Validate some fields
            // Because WorkItem.Fields is a dictionary keyed by "System.Title", "System.Description", etc.
            Assert.True(epicWorkItem.Fields.ContainsKey("System.Title"), "Title field not found in the read epic.");
            Assert.Equal("Read Epic Test", epicWorkItem.Fields["System.Title"]);

            Assert.True(epicWorkItem.Fields.ContainsKey("System.Description"), "Description field not found in the read epic.");
            Assert.Equal("This epic is for read test", epicWorkItem.Fields["System.Description"]);

            Assert.True(epicWorkItem.Fields.ContainsKey("System.Tags"), "Tags field not found in the read epic.");
            string? actualTags = epicWorkItem.Fields["System.Tags"].ToString();
            Assert.Contains("IntegrationTest", actualTags);
            Assert.Contains("Read", actualTags);
        }


        #region xUnit Lifetime

        /// <summary>
        /// xUnit method for async initialization before any tests run.
        /// Not needed here, so we return a completed task.
        /// </summary>
        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// xUnit method for async cleanup after all tests have finished.
        /// We delete all created work items in reverse order.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach(int id in _createdWorkItemIds.AsEnumerable().Reverse())
            {
                await _workItemsClient.DeleteWorkItemAsync(id);
            }
        }

        #endregion
    }
}
