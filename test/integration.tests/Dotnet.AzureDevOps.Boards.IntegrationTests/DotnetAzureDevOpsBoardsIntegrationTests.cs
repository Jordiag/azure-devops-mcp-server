using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class DotnetAzureDevOpsBoardsIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WorkItemsClient _workItemsClient;
        private readonly ReposClient _reposClient;
        private readonly ProjectSettingsClient _projectSettingsClient;
        private readonly IntegrationTestFixture _fixture;
        private readonly List<int> _createdWorkItemIds = [];
        private readonly List<int> _createdPullRequestIds = [];
        private readonly List<Guid> _createdProjectIds = [];
        private readonly string _repositoryName;
        private readonly string _sourceBranch;
        private readonly string _targetBranch;

        public DotnetAzureDevOpsBoardsIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _azureDevOpsConfiguration = fixture.Configuration;
            _workItemsClient = fixture.WorkItemsClient;
            _reposClient = fixture.ReposClient;
            _projectSettingsClient = fixture.ProjectSettingsClient;

            _repositoryName = _azureDevOpsConfiguration.RepoName;
            _sourceBranch = _azureDevOpsConfiguration.SrcBranch;
            _targetBranch = _azureDevOpsConfiguration.TargetBranch;
        }

        /// <summary>
        /// Test creating an Epic on its own.
        /// </summary>
        [Fact]
        public async Task CreateEpic_SucceedsAsync()
        {
            var options = new WorkItemCreateOptions
            {
                Title = "Integration Test Epic",
                Description = "Epic created by xUnit test",
                Tags = "IntegrationTest"
            };

            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(options);
            Assert.True(epicId.IsSuccessful, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);
        }

        /// <summary>
        /// Test creating a Feature that references a newly created Epic.
        /// </summary>
        [Fact]
        public async Task CreateFeature_SucceedsAsync()
        {
            // First, create an Epic so the Feature has a parent
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Feature Test",
                Description = "Parent epic for feature creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic (for Feature). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Now, create a Feature linked to that Epic
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Feature",
                Description = "Feature referencing epic",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });

            Assert.True(featureId.IsSuccessful, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);
        }

        /// <summary>
        /// Test creating a User Story that references a newly created Feature (which references an Epic).
        /// </summary>
        [Fact]
        public async Task CreateUserStory_SucceedsAsync()
        {
            // Create an Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Story Test",
                Description = "Parent epic for story creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic (for Story). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Create a Feature (child of Epic)
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Story Test",
                Description = "Parent feature for story creation",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.IsSuccessful, "Failed to create Feature (for Story). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // Create a User Story (child of Feature)
            AzureDevOpsActionResult<int> storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Story",
                Description = "Story referencing feature",
                ParentId = featureId.Value,
                Tags = "IntegrationTest"
            });

            Assert.True(storyId.IsSuccessful, "Failed to create User Story. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);
        }

        /// <summary>
        /// Test creating a Task that references a newly created User Story (which references a Feature, which references an Epic).
        /// </summary>
        [Fact]
        public async Task CreateTask_SucceedsAsync()
        {
            // Create an Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Task Test",
                Description = "Parent epic for task creation",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic (for Task). ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // Create a Feature
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Task Test",
                Description = "Parent feature for task creation",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.IsSuccessful, "Failed to create Feature (for Task). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // Create a User Story
            AzureDevOpsActionResult<int> storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story for Task Test",
                Description = "Parent story for task creation",
                ParentId = featureId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.IsSuccessful, "Failed to create User Story (for Task). ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // Finally, create a Task (child of the Story)
            AzureDevOpsActionResult<int> taskId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions
            {
                Title = "Integration Test Task",
                Description = "Task referencing user story",
                ParentId = storyId.Value,
                Tags = "IntegrationTest"
            });

            Assert.True(taskId.IsSuccessful, "Failed to create Task. ID was null.");
            _createdWorkItemIds.Add(taskId.Value);
        }

        /// <summary>
        /// Update an Epic: create it first, then modify fields and confirm update succeeded.
        /// </summary>
        [Fact]
        public async Task UpdateEpic_SucceedsAsync()
        {
            // 1. Create an Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic to Update",
                Description = "Original description",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic for update test. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Update the Epic
            AzureDevOpsActionResult<int> updatedId = await _workItemsClient.UpdateEpicAsync(epicId.Value, new WorkItemCreateOptions
            {
                Title = "Epic Updated Title",
                Description = "Updated description",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });

            Assert.True(updatedId.IsSuccessful, "Failed to update Epic. ID was null.");
            // updatedId should be the same as epicId, but we only store epicId in the list once
        }

        /// <summary>
        /// Update a Feature: create a parent Epic and the Feature, then modify fields on the Feature.
        /// </summary>
        [Fact]
        public async Task UpdateFeature_SucceedsAsync()
        {
            // 1. Create an Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Feature Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create the Feature referencing that Epic
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature to Update",
                Description = "Original feature",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.IsSuccessful, "Failed to create Feature (for update). ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Update the Feature
            AzureDevOpsActionResult<int> updatedId = await _workItemsClient.UpdateFeatureAsync(featureId.Value, new WorkItemCreateOptions
            {
                Title = "Feature Updated Title",
                Description = "Feature now updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.IsSuccessful, "Failed to update Feature. ID was null.");
        }

        /// <summary>
        /// Update a User Story: create an Epic, Feature, then a Story, then update the Story's fields.
        /// </summary>
        [Fact]
        public async Task UpdateUserStory_SucceedsAsync()
        {
            // 1. Create Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Story Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create Feature
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Story Update",
                Description = "Parent feature",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.IsSuccessful, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Create User Story
            AzureDevOpsActionResult<int> storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story to Update",
                Description = "Original story description",
                ParentId = featureId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.IsSuccessful, "Failed to create Story for update test. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // 4. Update the Story
            AzureDevOpsActionResult<int> updatedId = await _workItemsClient.UpdateUserStoryAsync(storyId.Value, new WorkItemCreateOptions
            {
                Title = "Story Updated Title",
                Description = "Story has been updated",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.IsSuccessful, "Failed to update Story. ID was null.");
        }

        /// <summary>
        /// Update a Task: create the chain (Epic -> Feature -> Story -> Task),
        /// then modify fields on the Task.
        /// </summary>
        [Fact]
        public async Task UpdateTask_SucceedsAsync()
        {
            // 1. Create Epic
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Epic for Task Update",
                Description = "Parent epic",
                Tags = "IntegrationTest"
            });
            Assert.True(epicId.IsSuccessful, "Failed to create Epic. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Create Feature
            AzureDevOpsActionResult<int> featureId = await _workItemsClient.CreateFeatureAsync(new WorkItemCreateOptions
            {
                Title = "Feature for Task Update",
                Description = "Parent feature",
                ParentId = epicId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(featureId.IsSuccessful, "Failed to create Feature. ID was null.");
            _createdWorkItemIds.Add(featureId.Value);

            // 3. Create User Story
            AzureDevOpsActionResult<int> storyId = await _workItemsClient.CreateUserStoryAsync(new WorkItemCreateOptions
            {
                Title = "Story for Task Update",
                Description = "Parent story",
                ParentId = featureId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(storyId.IsSuccessful, "Failed to create Story. ID was null.");
            _createdWorkItemIds.Add(storyId.Value);

            // 4. Create Task
            AzureDevOpsActionResult<int> taskId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions
            {
                Title = "Task to Update",
                Description = "Original task description",
                ParentId = storyId.Value,
                Tags = "IntegrationTest"
            });
            Assert.True(taskId.IsSuccessful, "Failed to create Task (for update). ID was null.");
            _createdWorkItemIds.Add(taskId.Value);

            // 5. Update the Task
            AzureDevOpsActionResult<int> updatedId = await _workItemsClient.UpdateTaskAsync(taskId.Value, new WorkItemCreateOptions
            {
                Title = "Task Updated Title",
                Description = "Updated task description",
                State = "Active",
                Tags = "IntegrationTest;Updated"
            });
            Assert.True(updatedId.IsSuccessful, "Failed to update Task. ID was null.");
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

            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(createOptions);
            Assert.True(epicId.IsSuccessful, "Failed to create Epic for read test. ID was null.");
            _createdWorkItemIds.Add(epicId.Value);

            // 2. Read the newly created Epic
            AzureDevOpsActionResult<WorkItem> epicWorkItem = await _workItemsClient.GetWorkItemAsync(epicId.Value);
            Assert.NotNull(epicWorkItem.Value); // we expect it to exist

            // 3. Validate some fields
            // Because WorkItem.Fields is a dictionary keyed by "System.Title", "System.Description", etc.
            Assert.True(epicWorkItem.Value.Fields.ContainsKey("System.Title"), "Title field not found in the read epic.");
            Assert.Equal("Read Epic Test", epicWorkItem.Value.Fields["System.Title"]);

            Assert.True(epicWorkItem.Value.Fields.ContainsKey("System.Description"), "Description field not found in the read epic.");
            Assert.Equal("This epic is for read test", epicWorkItem.Value.Fields["System.Description"]);

            Assert.True(epicWorkItem.Value.Fields.ContainsKey("System.Tags"), "Tags field not found in the read epic.");
            string? actualTags = epicWorkItem.Value.Fields["System.Tags"].ToString();
            Assert.Contains("IntegrationTest", actualTags);
            Assert.Contains("Read", actualTags);
        }

        /// <summary>
        /// Create an Epic and ensure WIQL query can locate it.
        /// </summary>
        [Fact]
        public async Task QueryWorkItems_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Query Epic",
                Tags = "IntegrationTest;Query"
            });
            Assert.True(epicId.IsSuccessful);
            _createdWorkItemIds.Add(epicId.Value);

            string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.Tags] CONTAINS 'Query'";
            AzureDevOpsActionResult<IReadOnlyList<WorkItem>> list = await _workItemsClient.QueryWorkItemsAsync(wiql);

            Assert.Contains(list.Value, w => w.Id == epicId.Value);
        }

        /// <summary>
        /// Add and retrieve a comment on a work item.
        /// </summary>
        [Fact]
        public async Task AddAndReadComments_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Comment Epic",
                Tags = "IntegrationTest;Comment"
            });
            Assert.True(epicId.IsSuccessful);
            _createdWorkItemIds.Add(epicId.Value);

            const string commentText = "Integration comment";
            AzureDevOpsActionResult<bool> addCommentResult = await _workItemsClient.AddCommentAsync(epicId.Value, _azureDevOpsConfiguration.ProjectName, commentText);
            Assert.True(addCommentResult.IsSuccessful && addCommentResult.Value);

            AzureDevOpsActionResult<IEnumerable<WorkItemComment>> commentsResult = await _workItemsClient.GetCommentsAsync(epicId.Value);
            Assert.True(commentsResult.IsSuccessful);
            IEnumerable<WorkItemComment> comments = commentsResult.Value;
            Assert.Contains(comments, c => c.Text == commentText);
        }

        /// <summary>
        /// Attach a file to a work item and download it again.
        /// </summary>
        [Fact]
        public async Task AttachAndDownload_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "Attachment Epic",
                Tags = "IntegrationTest;Attach"
            });
            Assert.True(epicId.IsSuccessful);
            _createdWorkItemIds.Add(epicId.Value);

            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "attachment");

            AzureDevOpsActionResult<Guid> attachmentResult = await _workItemsClient.AddAttachmentAsync(epicId.Value, tempFile);
            Assert.True(attachmentResult.IsSuccessful);
            Guid attachmentId = attachmentResult.Value;
            Assert.True(Guid.TryParse(attachmentId.ToString(), out _));


            AzureDevOpsActionResult<Stream> streamResult = await _workItemsClient.GetAttachmentAsync(_azureDevOpsConfiguration.ProjectName, attachmentId);
            Assert.True(streamResult.IsSuccessful);
            using Stream stream = streamResult.Value;
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
            AzureDevOpsActionResult<int> epicId = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions
            {
                Title = "History Epic",
                Description = "Initial",
                Tags = "IntegrationTest;History"
            });
            Assert.True(epicId.IsSuccessful);
            _createdWorkItemIds.Add(epicId.Value);

            await _workItemsClient.UpdateEpicAsync(epicId.Value, new WorkItemCreateOptions { Description = "Updated" });

            AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>> historyResult = await _workItemsClient.GetHistoryAsync(epicId.Value);
            Assert.True(historyResult.IsSuccessful);
            IReadOnlyList<WorkItemUpdate> history = historyResult.Value;
            Assert.True(history.Count > 1);
        }

        /// <summary>
        /// Link two work items and then remove the link.
        /// </summary>
        [Fact]
        public async Task LinkManagement_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> first = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Link A", Tags = "IntegrationTest;Link" });
            AzureDevOpsActionResult<int> second = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Link B", Tags = "IntegrationTest;Link" });
            Assert.True(first.IsSuccessful && second.IsSuccessful);
            _createdWorkItemIds.Add(first!.Value);
            _createdWorkItemIds.Add(second!.Value);

            await _workItemsClient.AddLinkAsync(first.Value, second.Value, "System.LinkTypes.Related");

            AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>> links = await _workItemsClient.GetLinksAsync(first.Value);
            Assert.Contains(links.Value, l => l.Url.Contains(second.Value.ToString()));

            string linkUrl = links.Value.First(l => l.Url.Contains(second.Value.ToString())).Url!;
            await _workItemsClient.RemoveLinkAsync(first.Value, linkUrl);

            AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>> after = await _workItemsClient.GetLinksAsync(first.Value);
            Assert.DoesNotContain(after.Value, l => l.Url == linkUrl);
        }

        [Fact]
        public async Task BulkEdit_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> a = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Bulk 1", Tags = "IntegrationTest;Bulk" });
            AzureDevOpsActionResult<int> b = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Bulk 2", Tags = "IntegrationTest;Bulk" });
            Assert.True(a.IsSuccessful && b.IsSuccessful);
            _createdWorkItemIds.Add(a!.Value);
            _createdWorkItemIds.Add(b!.Value);

            var updates = new (int, WorkItemCreateOptions)[]
            {
                (a.Value, new WorkItemCreateOptions { State = "Closed" }),
                (b.Value, new WorkItemCreateOptions { State = "Closed" })
            };

            await _workItemsClient.BulkUpdateWorkItemsAsync(updates);

            AzureDevOpsActionResult<WorkItem> first = await _workItemsClient.GetWorkItemAsync(a.Value);
            AzureDevOpsActionResult<WorkItem> second = await _workItemsClient.GetWorkItemAsync(b.Value);
            Assert.Equal("Closed", first?.Value.Fields?["System.State"].ToString());
            Assert.Equal("Closed", second?.Value.Fields?["System.State"].ToString());
        }

        [Fact]
        public async Task ListBoards_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<BoardReference>> boardRefsResult = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.True(boardRefsResult.IsSuccessful);
            IReadOnlyList<BoardReference> boardReferences = boardRefsResult.Value;
            Assert.NotEmpty(boardReferences);
        }

        [Fact]
        public async Task GetTeamIteration_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> iterationsResult = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
            Assert.True(iterationsResult.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterations = iterationsResult.Value;
            Assert.NotEmpty(iterations);

            TeamSettingsIteration iteration = iterations.First();
            AzureDevOpsActionResult<TeamSettingsIteration> iterationResult = await _workItemsClient.GetTeamIterationAsync(teamContext, iteration.Id);
            Assert.True(iterationResult.IsSuccessful);
            TeamSettingsIteration fetched = iterationResult.Value;
            Assert.Equal(iteration.Id, fetched.Id);
        }

        [Fact]
        public async Task GetTeamIterations_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> iterationsResult2 = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
            Assert.True(iterationsResult2.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterations = iterationsResult2.Value;
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task ListBoardColumns_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<BoardReference>> boardsResult = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.True(boardsResult.IsSuccessful);
            IReadOnlyList<BoardReference> boards = boardsResult.Value;
            Assert.NotEmpty(boards);

            Guid boardId = boards.First().Id;
            AzureDevOpsActionResult<IReadOnlyList<BoardColumn>> columnsResult = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardId);
            Assert.True(columnsResult.IsSuccessful);
            IReadOnlyList<BoardColumn> columns = columnsResult.Value;
            Assert.NotEmpty(columns);
        }

        /// <summary>
        /// Project name and team name are required for this test.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListBacklogs_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, "Dotnet.McpIntegrationTest Team");
            AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>> backlogsResult = await _workItemsClient.ListBacklogsAsync(teamContext);
            Assert.True(backlogsResult.IsSuccessful);
            IReadOnlyList<BacklogLevelConfiguration> backlogs = backlogsResult.Value;
            Assert.NotEmpty(backlogs);
        }

        /// <summary>
        /// Project name and team name are required for this test.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListBacklogWorkItems_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, "Dotnet.McpIntegrationTest Team");
            AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>> backlogsResult2 = await _workItemsClient.ListBacklogsAsync(teamContext);
            Assert.True(backlogsResult2.IsSuccessful);
            IReadOnlyList<BacklogLevelConfiguration> backlogs = backlogsResult2.Value;
            Assert.NotEmpty(backlogs);

            string backlogId = backlogs.First().Id!;
            AzureDevOpsActionResult<BacklogLevelWorkItems> backlogItemsResult = await _workItemsClient.ListBacklogWorkItemsAsync(teamContext, backlogId);
            Assert.True(backlogItemsResult.IsSuccessful);
            BacklogLevelWorkItems backlogItems = backlogItemsResult.Value;
            Assert.NotNull(backlogItems);
        }

        [Fact]
        public async Task ListMyWorkItems_SucceedsAsync()
        {
            AzureDevOpsActionResult<PredefinedQuery> queryResult = await _workItemsClient.ListMyWorkItemsAsync();
            Assert.True(queryResult.IsSuccessful);
            PredefinedQuery query = queryResult.Value;
            Assert.NotNull(query);
        }

        [Fact]
        public async Task LinkWorkItemToPullRequest_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> workItemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "PR Link", Tags = "IntegrationTest;PR" });
            Assert.True(workItemId.IsSuccessful);
            _createdWorkItemIds.Add(workItemId!.Value);
            string sourceBranch = $"{_sourceBranch}2";
            AzureDevOpsActionResult<GitRef> gitRef = await _reposClient.GetBranchAsync(_repositoryName, sourceBranch);

            if(string.IsNullOrEmpty(gitRef?.Value?.Name))
            {
                string targetBranchName = _targetBranch.Replace("refs/heads/", string.Empty);
                AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommits = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repositoryName,
                    targetBranchName,
                    top: 1);

                if(latestCommits?.Value?.Count == 0)
                    return;

                string? commitSha = latestCommits?.Value?[0].CommitId!;
                await _reposClient.CreateBranchAsync(_repositoryName, sourceBranch, commitSha);
            }

            var prOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repositoryName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR for link test",
                SourceBranch = sourceBranch,
                TargetBranch = _targetBranch,
                IsDraft = false
            };

            AzureDevOpsActionResult<int> pullRequestId = await _reposClient.CreatePullRequestAsync(prOptions);
            Assert.True(pullRequestId.Value > 0);
            _createdPullRequestIds.Add(pullRequestId!.Value);

            await _workItemsClient.LinkWorkItemToPullRequestAsync(
                _azureDevOpsConfiguration.ProjectId,
                _azureDevOpsConfiguration.RepositoryId,
                pullRequestId.Value,
                workItemId.Value);
        }

        [Fact]
        public async Task GetWorkItemsForIteration_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> listIterationsResult = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.True(listIterationsResult.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterations = listIterationsResult.Value;
            Assert.NotEmpty(iterations);

            TeamSettingsIteration iteration = iterations.First();
            AzureDevOpsActionResult<IterationWorkItems> iterationItemsResult = await _workItemsClient.GetWorkItemsForIterationAsync(teamContext, iteration.Id);
            Assert.True(iterationItemsResult.IsSuccessful);
            IterationWorkItems result = iterationItemsResult.Value;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListIterations_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> listIterationsResult2 = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.True(listIterationsResult2.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterations = listIterationsResult2.Value;
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task CreateIterations_SucceedsAsync()
        {
            string name = $"it-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var iterations = new List<IterationCreateOptions>
            {
                new() { IterationName = name }
            };

            AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>> created = await _workItemsClient.CreateIterationsAsync(_azureDevOpsConfiguration.ProjectName, iterations);
            Assert.NotEmpty(created.Value);
        }

        [Fact]
        public async Task AssignIterations_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> existingResult = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.True(existingResult.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> existing = existingResult.Value;
            Assert.NotEmpty(existing);

            TeamSettingsIteration iteration = existing.First();
            var assignments = new List<IterationAssignmentOptions>
            {
                new() { Identifier = iteration.Id, Path = iteration.Path! }
            };

            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> result = await _workItemsClient.AssignIterationsAsync(teamContext, assignments);
            Assert.NotEmpty(result.Value);
        }

        [Fact]
        public async Task ListAreas_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<TeamFieldValues> areasResult = await _workItemsClient.ListAreasAsync(teamContext);
            Assert.True(areasResult.IsSuccessful);
            TeamFieldValues areas = areasResult.Value;
            Assert.NotEmpty(areas.Values);
        }

        // TODO: Re-enable this test once the API is working again
        [Fact(Skip = "API not longer working")]
        public async Task CreateCustomFieldIfDoesNotExist_SucceedsAsync()
        {
            WorkItemsClient client = _workItemsClient;
            string fieldName = $"CustomField{UtcStamp()}".Replace(".", "").Replace("-", "");
            ;
            string referenceName = $"Custom.Reference.{UtcStamp()}".Replace(".", "").Replace("-", "");
            ;

            AzureDevOpsActionResult<bool> isSystemProcess = await _workItemsClient.IsSystemProcessAsync();
            if(isSystemProcess.Value)
            {
                string processName = $"it-proc-{UtcStamp()}";
                AzureDevOpsActionResult<bool> processCreatedResult = await _projectSettingsClient.CreateInheritedProcessAsync(processName, "Custom", "Agile");
                Assert.True(processCreatedResult.IsSuccessful && processCreatedResult.Value);
                string? processId = null;
                await WaitHelper.WaitUntilAsync(async () =>
                {
                    AzureDevOpsActionResult<string> processIdResult = await _projectSettingsClient.GetProcessIdAsync(processName);
                    processId = processIdResult.Value;
                    return !string.IsNullOrEmpty(processId);
                }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
                Assert.False(string.IsNullOrEmpty(processId));

                string projectName = $"it-proj-{UtcStamp()}";
                AzureDevOpsActionResult<Guid> projectIdResult = await _projectSettingsClient.CreateProjectAsync(projectName, "Custom field project", processId!);
                Assert.True(projectIdResult.IsSuccessful);
                Guid projectId = projectIdResult.Value;
                _createdProjectIds.Add(projectId);

                HttpClient httpClient = _fixture.CreateHttpClient(_azureDevOpsConfiguration.OrganisationUrl);
                
                client = new WorkItemsClient(
                    httpClient,
                    _azureDevOpsConfiguration.OrganisationUrl,
                    projectName,
                    _azureDevOpsConfiguration.PersonalAccessToken);
            }

            AzureDevOpsActionResult<WorkItemField2> first = await client.CreateCustomFieldIfDoesntExistAsync(
                fieldName,
                referenceName,
                Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType.String,
                "integration test field");
            Assert.Equal(referenceName, first.Value.ReferenceName);

            AzureDevOpsActionResult<WorkItemField2> second = await client.CreateCustomFieldIfDoesntExistAsync(
                fieldName,
                referenceName,
                Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType.String,
                "integration test field");
            Assert.Equal(referenceName, second.Value.ReferenceName);
        }

        /// <summary>
        /// Requires a custom process to be created first, as it uses a custom field.
        /// </summary>
        /// <returns></returns>
        // TODO: Re-enable this test once the API is working again
        [Fact(Skip = "API not longer working")]
        public async Task CustomFieldWorkflow_SucceedsAsync()
        {
            WorkItemsClient client = _workItemsClient;
            string fieldName = "CustomIntegrationTestField";
            string referenceName = "TestField.ForIntegration";

            AzureDevOpsActionResult<bool> isSystemProcess = await _workItemsClient.IsSystemProcessAsync();
            if(isSystemProcess.Value)
            {
                string processName = $"it-proc-{UtcStamp()}";
                AzureDevOpsActionResult<bool> processCreatedResult = await _projectSettingsClient.CreateInheritedProcessAsync(processName, "Custom", "Agile");
                Assert.True(processCreatedResult.IsSuccessful && processCreatedResult.Value);

                string? processId = null;
                await WaitHelper.WaitUntilAsync(async () =>
                {
                    AzureDevOpsActionResult<string> processIdResult = await _projectSettingsClient.GetProcessIdAsync(processName);
                    processId = processIdResult.Value;
                    return processId != null;
                }, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2));

                Assert.False(string.IsNullOrEmpty(processId));

                string projectName = $"it-proj-{UtcStamp()}";
                AzureDevOpsActionResult<Guid> projectIdResult = await _projectSettingsClient.CreateProjectAsync(projectName, "Custom field project", processId!);
                Assert.True(projectIdResult.IsSuccessful);
                Guid projectId = projectIdResult.Value;
                _createdProjectIds.Add(projectId);

                HttpClient httpClient = _fixture.CreateHttpClient(_azureDevOpsConfiguration.OrganisationUrl);
                
                client = new WorkItemsClient(
                    httpClient,
                    _azureDevOpsConfiguration.OrganisationUrl,
                    projectName,
                    _azureDevOpsConfiguration.PersonalAccessToken);
            }

            AzureDevOpsActionResult<int> workItemId = await client.CreateTaskAsync(new WorkItemCreateOptions { Title = "Custom Field" });
            Assert.True(workItemId.IsSuccessful);
            _createdWorkItemIds.Add(workItemId!.Value);

            await client.CreateCustomFieldIfDoesntExistAsync(fieldName, referenceName, Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType.String, "test field");

            AzureDevOpsActionResult<WorkItem> workItem = await client.SetCustomFieldAsync(workItemId.Value, fieldName, "Value1");
            AzureDevOpsActionResult<object> fieldValue = await client.GetCustomFieldAsync(workItemId.Value, referenceName);
            Assert.NotNull(fieldValue.Value);
        }

        [Fact]
        public async Task ExportBoard_SucceedsAsync()
        {
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<IReadOnlyList<BoardReference>> boardsResult2 = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.True(boardsResult2.IsSuccessful);
            IReadOnlyList<BoardReference> boards = boardsResult2.Value;
            Assert.NotEmpty(boards);

            AzureDevOpsActionResult<Board> exportResult = await _workItemsClient.ExportBoardAsync(teamContext, boards.First().Id.ToString());
            Assert.True(exportResult.IsSuccessful);
            Board board = exportResult.Value;
            Assert.NotNull(board);
        }

        [Fact]
        public async Task GetWorkItemCount_SucceedsAsync()
        {
            string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project";
            AzureDevOpsActionResult<int> count = await _workItemsClient.GetWorkItemCountAsync(wiql);
            Assert.True(count.Value >= 0);
        }

        [Fact]
        public async Task ExecuteBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> id = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Root" });
            Assert.True(id.IsSuccessful);
            _createdWorkItemIds.Add(id!.Value);

            var request = new WitBatchRequest
            {
                Method = "GET",
                Uri = $"/_apis/wit/workitems/{id.Value}?api-version={GlobalConstants.ApiVersion}"
            };

            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> responses = await _workItemsClient.ExecuteBatchAsync(new[] { request });
            Assert.NotEmpty(responses.Value);
        }

        [Fact]
        public async Task CreateWorkItemsBatch_SucceedsAsync()
        {
            var items = new List<WorkItemCreateOptions>
            {
                new() { Title = "Batch Item 1" },
                new() { Title = "Batch Item 2" }
            };

            AzureDevOpsActionResult<IReadOnlyList<int>> created = await _workItemsClient.CreateWorkItemsMultipleCallsAsync("Task", items, CancellationToken.None);
            Assert.Equal(2, created.Value.Count);
            foreach(int workItem in created.Value)
            {
                _createdWorkItemIds.Add(workItem);
            }
        }

        [Fact]
        public async Task UpdateWorkItemsBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> firstId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Update 1" });
            AzureDevOpsActionResult<int> secondId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Update 2" });
            Assert.True(firstId.IsSuccessful && secondId.IsSuccessful);
            _createdWorkItemIds.Add(firstId!.Value);
            _createdWorkItemIds.Add(secondId!.Value);

            var updates = new List<(int, WorkItemCreateOptions)>
            {
                (firstId.Value, new WorkItemCreateOptions { State = "Closed" }),
                (secondId.Value, new WorkItemCreateOptions { State = "Closed" })
            };

            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> batch = await _workItemsClient.UpdateWorkItemsBatchAsync(updates);
            Assert.Equal(2, batch.Value.Count);
        }

        [Fact]
        public async Task LinkWorkItemsBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> parentId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Parent" });
            AzureDevOpsActionResult<int> childId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Child" });
            Assert.True(parentId.IsSuccessful && childId.IsSuccessful);
            _createdWorkItemIds.Add(parentId!.Value);
            _createdWorkItemIds.Add(childId!.Value);

            var links = new List<(int, int, string)> { (parentId.Value, childId.Value, "System.LinkTypes.Related") };

            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> responsesResult = await _workItemsClient.LinkWorkItemsBatchAsync(links);
            Assert.True(responsesResult.IsSuccessful);
            IReadOnlyList<WitBatchResponse> responses = responsesResult.Value;
            Assert.NotEmpty(responses);
        }


        [Fact]
        public async Task CloseWorkItemsBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> id1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Close 1" });
            AzureDevOpsActionResult<int> id2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Close 2" });
            Assert.True(id1.IsSuccessful && id2.IsSuccessful);
            _createdWorkItemIds.Add(id1!.Value);
            _createdWorkItemIds.Add(id2!.Value);


            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> closeResponsesResult = await _workItemsClient.CloseWorkItemsBatchAsync(new[] { id1.Value, id2.Value });
            Assert.True(closeResponsesResult.IsSuccessful);
            IReadOnlyList<WitBatchResponse> responses = closeResponsesResult.Value;
            Assert.Equal(2, responses.Count);
        }

        [Fact]
        public async Task CloseAndLinkDuplicatesBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> canonical = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Canonical" });
            AzureDevOpsActionResult<int> duplicate = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Duplicate" });
            Assert.True(canonical.IsSuccessful && duplicate.IsSuccessful);
            _createdWorkItemIds.Add(canonical!.Value);
            _createdWorkItemIds.Add(duplicate!.Value);

            var pairs = new List<(int, int)> { (duplicate.Value, canonical.Value) };

            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> closeDupResult = await _workItemsClient.CloseAndLinkDuplicatesBatchAsync(pairs);
            Assert.True(closeDupResult.IsSuccessful);
            IReadOnlyList<WitBatchResponse> responses = closeDupResult.Value;
            Assert.Single(responses);
        }

        [Fact]
        public async Task GetWorkItemsBatchByIds_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> item1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "BatchGet 1" });
            AzureDevOpsActionResult<int> item2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "BatchGet 2" });
            Assert.True(item1.IsSuccessful && item2.IsSuccessful);
            _createdWorkItemIds.Add(item1!.Value);
            _createdWorkItemIds.Add(item2!.Value);

            AzureDevOpsActionResult<IReadOnlyList<WorkItem>> itemsResult = await _workItemsClient.GetWorkItemsBatchByIdsAsync(new[] { item1.Value, item2.Value });
            Assert.True(itemsResult.IsSuccessful);
            IReadOnlyList<WorkItem> items = itemsResult.Value;
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateWorkItem_SucceedsAsync()
        {
            var fields = new List<WorkItemFieldValue>
            {
                new() { Name = "System.Title", Value = "Arbitrary" }
            };

            AzureDevOpsActionResult<WorkItem> workItem = await _workItemsClient.CreateWorkItemAsync("Task", fields);
            Assert.NotNull(workItem);
            if(workItem.Value.Id != null)
            {
                _createdWorkItemIds.Add(workItem.Value.Id.Value);
            }
        }

        [Fact]
        public async Task UpdateWorkItem_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> itemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Update Arbitrary" });
            Assert.True(itemId.IsSuccessful);
            _createdWorkItemIds.Add(itemId!.Value);

            var updates = new List<Core.Boards.Options.WorkItemFieldUpdate>
            {
                new() { Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add, Path = "/fields/System.Title", Value = "Updated" }
            };

            AzureDevOpsActionResult<WorkItem> updated = await _workItemsClient.UpdateWorkItemAsync(itemId.Value, updates);
            Assert.True(updated.IsSuccessful);
            Assert.NotNull(updated.Value);
        }

        [Fact]
        public async Task GetWorkItemType_SucceedsAsync()
        {
            AzureDevOpsActionResult<WorkItemType> type = await _workItemsClient.GetWorkItemTypeAsync(_azureDevOpsConfiguration.ProjectName, "Task");
            Assert.NotNull(type.Value);
        }

        [Fact]
        public async Task GetQuery_SucceedsAsync()
        {
            AzureDevOpsActionResult<QueryHierarchyItem> query = await _workItemsClient.GetQueryAsync(_azureDevOpsConfiguration.ProjectName, "Shared Queries");
            Assert.NotNull(query.Value);
        }

        [Fact]
        public async Task GetQueryResultsById_SucceedsAsync()
        {

            string queryName = "My Test Query";
            string wiql = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.WorkItemType] = 'Task'";
            await _workItemsClient.CreateSharedQueryAsync(_azureDevOpsConfiguration.ProjectName, queryName, wiql);

            AzureDevOpsActionResult<QueryHierarchyItem> root = await _workItemsClient.GetQueryAsync(
                _azureDevOpsConfiguration.ProjectName,
                "Shared Queries",
                depth: 2); // Increase depth to capture nested items

            QueryHierarchyItem? queryItem = FindFirstQuery(root.Value);

            Assert.NotNull(queryItem); // Fail fast if no valid query is found

            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            AzureDevOpsActionResult<WorkItemQueryResult> result = await _workItemsClient.GetQueryResultsByIdAsync(queryItem!.Id, teamContext);

            Assert.NotNull(result.Value);

            await _workItemsClient.DeleteSharedQueryAsync(_azureDevOpsConfiguration.ProjectName, queryName);
        }

        private QueryHierarchyItem? FindFirstQuery(QueryHierarchyItem root)
        {
            var stack = new Stack<QueryHierarchyItem>();
            stack.Push(root);

            while(stack.Count > 0)
            {
                QueryHierarchyItem current = stack.Pop();

                if(current.IsFolder == false || current.IsFolder == null)
                {
                    return current;
                }

                if(current.Children != null)
                {
                    for(int i = current.Children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.Children[i]);
                    }
                }
            }

            return null;
        }

        [Fact]
        public async Task LinkWorkItemsByNameBatch_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> w1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "LinkName1" });
            AzureDevOpsActionResult<int> w2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "LinkName2" });
            Assert.True(w1.IsSuccessful && w2.IsSuccessful);
            _createdWorkItemIds.Add(w1!.Value);
            _createdWorkItemIds.Add(w2!.Value);

            var links = new List<(int, int, string, string?)>
            {
                (w1.Value, w2.Value, "related", "link")
            };

            AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> respResult = await _workItemsClient.LinkWorkItemsByNameBatchAsync(links);
            Assert.True(respResult.IsSuccessful);
            IReadOnlyList<WitBatchResponse> resp = respResult.Value;
            Assert.Single(resp);
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

            foreach(int prId in _createdPullRequestIds.AsEnumerable().Reverse())
            {
                await _reposClient.AbandonPullRequestAsync(_repositoryName, prId);
            }

            foreach(Guid projectId in _createdProjectIds.AsEnumerable().Reverse())
            {
                await _projectSettingsClient.DeleteProjectAsync(projectId);
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("O").Replace(':', '-');
    }
}
