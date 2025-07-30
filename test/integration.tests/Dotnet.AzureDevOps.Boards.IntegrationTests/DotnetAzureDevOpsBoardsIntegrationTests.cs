using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class DotnetAzureDevOpsBoardsIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WorkItemsClient _workItemsClient;
        private readonly List<int> _createdWorkItemIds = [];

        public DotnetAzureDevOpsBoardsIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

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

        /// <summary>
        /// Create an Epic and ensure WIQL query can locate it.
        /// </summary>
        [Fact]
        public async Task QueryWorkItems_SucceedsAsync()
        {
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Query Epic",
                Tags = "IntegrationTest;Query"
            });
            Assert.True(epicId.HasValue);
            _createdWorkItemIds.Add(epicId.Value);

            string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.Tags] CONTAINS 'Query'";
            IReadOnlyList<WorkItem> list = await _workItemsClient.QueryWorkItemsAsync(wiql);

            Assert.Contains(list, w => w.Id == epicId.Value);
        }

        /// <summary>
        /// Add and retrieve a comment on a work item.
        /// </summary>
        [Fact]
        public async Task AddAndReadComments_SucceedsAsync()
        {
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Comment Epic",
                Tags = "IntegrationTest;Comment"
            });
            Assert.True(epicId.HasValue);
            _createdWorkItemIds.Add(epicId.Value);

            const string commentText = "Integration comment";
            await _workItemsClient.AddCommentAsync(epicId.Value, _azureDevOpsConfiguration.ProjectName, commentText);

            IReadOnlyList<WorkItemComment> comments = await _workItemsClient.GetCommentsAsync(epicId.Value);
            Assert.Contains(comments, c => c.Text == commentText);
        }

        /// <summary>
        /// Attach a file to a work item and download it again.
        /// </summary>
        [Fact]
        public async Task AttachAndDownload_SucceedsAsync()
        {
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Attachment Epic",
                Tags = "IntegrationTest;Attach"
            });
            Assert.True(epicId.HasValue);
            _createdWorkItemIds.Add(epicId.Value);

            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "attachment");

            Guid? attachmentId = await _workItemsClient.AddAttachmentAsync(epicId.Value, tempFile);
            Assert.NotNull(attachmentId);

            using Stream? stream = await _workItemsClient.GetAttachmentAsync(_azureDevOpsConfiguration.ProjectName, attachmentId.Value);
            Assert.NotNull(stream);

            long len;
            using(var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                len = memoryStream.Length;
            }
            Assert.True(len > 0);

            File.Delete(tempFile);
        }

        /// <summary>
        /// Verify history records after updating a work item.
        /// </summary>
        [Fact]
        public async Task WorkItemHistory_SucceedsAsync()
        {
            int? epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "History Epic",
                Description = "Initial",
                Tags = "IntegrationTest;History"
            });
            Assert.True(epicId.HasValue);
            _createdWorkItemIds.Add(epicId.Value);

            await _workItemsClient.UpdateEpicAsync(epicId.Value, new WorkItemCreateOptions { Description = "Updated" });

            IReadOnlyList<WorkItemUpdate> history = await _workItemsClient.GetHistoryAsync(epicId.Value);
            Assert.True(history.Count > 1);
        }

        /// <summary>
        /// Link two work items and then remove the link.
        /// </summary>
        [Fact]
        public async Task LinkManagement_SucceedsAsync()
        {
            int? first = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Link A", Tags = "IntegrationTest;Link" });
            int? second = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Link B", Tags = "IntegrationTest;Link" });
            Assert.True(first.HasValue && second.HasValue);
            _createdWorkItemIds.Add(first!.Value);
            _createdWorkItemIds.Add(second!.Value);

            await _workItemsClient.AddLinkAsync(first.Value, second.Value, "System.LinkTypes.Related");

            IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(first.Value);
            Assert.Contains(links, l => l.Url.Contains(second.Value.ToString()));

            string linkUrl = links.First(l => l.Url.Contains(second.Value.ToString())).Url!;
            await _workItemsClient.RemoveLinkAsync(first.Value, linkUrl);

            IReadOnlyList<WorkItemRelation> after = await _workItemsClient.GetLinksAsync(first.Value);
            Assert.DoesNotContain(after, l => l.Url == linkUrl);
        }

        [Fact]
        public async Task BulkEdit_SucceedsAsync()
        {
            int? a = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Bulk 1", Tags = "IntegrationTest;Bulk" });
            int? b = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Bulk 2", Tags = "IntegrationTest;Bulk" });
            Assert.True(a.HasValue && b.HasValue);
            _createdWorkItemIds.Add(a!.Value);
            _createdWorkItemIds.Add(b!.Value);

            var updates = new (int, WorkItemCreateOptions)[]
            {
                (a.Value, new WorkItemCreateOptions { State = "Closed" }),
                (b.Value, new WorkItemCreateOptions { State = "Closed" })
            };

            await _workItemsClient.BulkUpdateWorkItemsAsync(updates);

            WorkItem? first = await _workItemsClient.GetWorkItemAsync(a.Value);
            WorkItem? second = await _workItemsClient.GetWorkItemAsync(b.Value);
            Assert.Equal("Closed", first?.Fields?["System.State"].ToString());
            Assert.Equal("Closed", second?.Fields?["System.State"].ToString());
        }

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
    }
}
