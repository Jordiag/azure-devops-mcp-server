using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class DotnetAzureDevOpsBoardsIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly WorkItemsClient _workItemsClient;
        private readonly ReposClient _reposClient;
        private readonly List<int> _createdWorkItemIds = [];
        private readonly List<int> _createdPullRequestIds = [];
        private readonly string _repositoryName;
        private readonly string _sourceBranch;
        private readonly string _targetBranch;

        public DotnetAzureDevOpsBoardsIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

            _workItemsClient = new WorkItemsClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);

            _reposClient = new ReposClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);

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

            int? epicId = await _workItemsClient.CreateEpicAsync(options);
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

        [Fact]
        public async Task ListBoards_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<BoardReference> boardReferences = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boardReferences);
        }

        [Fact]
        public async Task GetTeamIteration_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
            Assert.NotEmpty(iterations);

            TeamSettingsIteration iteration = iterations.First();
            TeamSettingsIteration fetched = await _workItemsClient.GetTeamIterationAsync(teamContext, iteration.Id);
            Assert.Equal(iteration.Id, fetched.Id);
        }

        [Fact]
        public async Task GetTeamIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task ListBoardColumns_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<BoardReference> boards = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boards);

            Guid boardId = boards.First().Id;
            List<BoardColumn> columns = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardId);
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
            List<BacklogLevelConfiguration> backlogs = await _workItemsClient.ListBacklogsAsync(teamContext);
            Assert.NotEmpty(backlogs);
        }

        /// <summary>
        /// Project name and team name are required for this test.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListBacklogWorkItems_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, "Dotnet.McpIntegrationTest Team");
            List<BacklogLevelConfiguration> backlogs = await _workItemsClient.ListBacklogsAsync(teamContext);
            Assert.NotEmpty(backlogs);

            string backlogId = backlogs.First().Id!;
            BacklogLevelWorkItems backlogItems = await _workItemsClient.ListBacklogWorkItemsAsync(teamContext, backlogId);
            Assert.NotNull(backlogItems);
        }

        [Fact]
        public async Task ListMyWorkItems_SucceedsAsync()
        {
            PredefinedQuery query = await _workItemsClient.ListMyWorkItemsAsync();
            Assert.NotNull(query);
        }

        /// <summary>
        /// TODO: This test is flaky on CI, needs investigation.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Flaky on CI - will fix later")]
        public async Task LinkWorkItemToPullRequest_SucceedsAsync()
        {
            int? workItemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "PR Link", Tags = "IntegrationTest;PR" });
            Assert.True(workItemId.HasValue);
            _createdWorkItemIds.Add(workItemId!.Value);

            var prOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repositoryName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR for link test",
                SourceBranch = _sourceBranch,
                TargetBranch = _targetBranch,
                IsDraft = false
            };

            int? pullRequestId = await _reposClient.CreatePullRequestAsync(prOptions);
            Assert.True(pullRequestId.HasValue);
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
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<TeamSettingsIteration> iterations = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(iterations);

            TeamSettingsIteration iteration = iterations.First();
            IterationWorkItems result = await _workItemsClient.GetWorkItemsForIterationAsync(teamContext, iteration.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<TeamSettingsIteration> iterations = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task CreateIterations_SucceedsAsync()
        {
            string name = $"it-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var iterations = new List<IterationCreateOptions>
            {
                new IterationCreateOptions { IterationName = name }
            };

            IReadOnlyList<WorkItemClassificationNode> created = await _workItemsClient.CreateIterationsAsync(_azureDevOpsConfiguration.ProjectName, iterations);
            Assert.NotEmpty(created);
        }

        [Fact]
        public async Task AssignIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<TeamSettingsIteration> existing = await _workItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(existing);

            TeamSettingsIteration iteration = existing.First();
            var assignments = new List<IterationAssignmentOptions>
            {
                new IterationAssignmentOptions { Identifier = iteration.Id, Path = iteration.Path! }
            };

            IReadOnlyList<TeamSettingsIteration> result = await _workItemsClient.AssignIterationsAsync(teamContext, assignments);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ListAreas_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            TeamFieldValues areas = await _workItemsClient.ListAreasAsync(teamContext);
            Assert.NotEmpty(areas.Values);
        }

        /// <summary>
        /// TODO: create a non system project before
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CustomFieldWorkflow_SucceedsAsync()
        {
            if(!await _workItemsClient.IsSystemProcessAsync())
            {
                int? workItemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Custom Field" });
                Assert.True(workItemId.HasValue);
                _createdWorkItemIds.Add(workItemId!.Value);

                await _workItemsClient.SetCustomFieldAsync(workItemId.Value, "Custom.TestField", "Value1");
                object? fieldValue = await _workItemsClient.GetCustomFieldAsync(workItemId.Value, "Custom.TestField");
                Assert.NotNull(fieldValue);
            }
            else
            {
                Assert.True(true, "Skipping custom field test for system process.");
            }
        }

        [Fact]
        public async Task ExportBoard_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            List<BoardReference> boards = await _workItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boards);

            Board? board = await _workItemsClient.ExportBoardAsync(teamContext, boards.First().Id.ToString());
            Assert.NotNull(board);
        }

        [Fact]
        public async Task GetWorkItemCount_SucceedsAsync()
        {
            string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project";
            int count = await _workItemsClient.GetWorkItemCountAsync(wiql);
            Assert.True(count >= 0);
        }

        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ExecuteBatch_SucceedsAsync()
        {
            int? id = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Root" });
            Assert.True(id.HasValue);
            _createdWorkItemIds.Add(id!.Value);

            var request = new WitBatchRequest
            {
                Method = "GET",
                Uri = $"/_apis/wit/workitems/{id.Value}?api-version={GlobalConstants.ApiVersion}"
            };

            try
            {
                IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.ExecuteBatchAsync(new[] { request });
                Assert.NotEmpty(responses);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
        }

        [Fact]
        public async Task CreateWorkItemsBatch_SucceedsAsync()
        {
            var items = new List<WorkItemCreateOptions>
            {
                new() { Title = "Batch Item 1" },
                new() { Title = "Batch Item 2" }
            };

            IReadOnlyList<int> created = await _workItemsClient.CreateWorkItemsBatchAsync("Task", items, CancellationToken.None);
            Assert.Equal(2, created.Count);
            foreach(int workItem in created)
            {
                _createdWorkItemIds.Add(workItem);
            }
        }

        /// <summary>
        /// TODO: correct PAT user so doesn't raise exception about permissions.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateWorkItemsBatchViaBatch_SucceedsAsync()
        {
            var items = new List<WorkItemCreateOptions>
            {
                new WorkItemCreateOptions { Title = "Batch API 1" },
                new WorkItemCreateOptions { Title = "Batch API 2" }
            };

            try
            {
                IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.CreateWorkItemsBatchAsync("Task", items, true, false);
                foreach(WitBatchResponse response in responses)
                {
                    WorkItem? created = JsonSerializer.Deserialize<WorkItem>(response.Body?.ToString() ?? "{}");
                    if(created?.Id != null)
                    {
                        _createdWorkItemIds.Add(created.Id.Value);
                    }
                }
                Assert.Equal(2, responses.Count);
            }
            catch(VssServiceException ex)
            {

                Assert.Contains("TF237111: The current user does not have permissions to save work items under the specified area path.", ex.Message);
            }
        }

        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateWorkItemsBatch_SucceedsAsync()
        {
            int? firstId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Update 1" });
            int? secondId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Update 2" });
            Assert.True(firstId.HasValue && secondId.HasValue);
            _createdWorkItemIds.Add(firstId!.Value);
            _createdWorkItemIds.Add(secondId!.Value);

            var updates = new List<(int, WorkItemCreateOptions)>
            {
                (firstId.Value, new WorkItemCreateOptions { State = "Closed" }),
                (secondId.Value, new WorkItemCreateOptions { State = "Closed" })
            };

            try
            {
                IReadOnlyList<WitBatchResponse> batch = await _workItemsClient.UpdateWorkItemsBatchAsync(updates);
                Assert.Equal(2, batch.Count);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
        }

        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkWorkItemsBatch_SucceedsAsync()
        {
            int? parentId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Parent" });
            int? childId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Batch Child" });
            Assert.True(parentId.HasValue && childId.HasValue);
            _createdWorkItemIds.Add(parentId!.Value);
            _createdWorkItemIds.Add(childId!.Value);

            var links = new List<(int, int, string)> { (parentId.Value, childId.Value, "System.LinkTypes.Related") };

            try
            {
                IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.LinkWorkItemsBatchAsync(links);
                Assert.NotEmpty(responses);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
        }


        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseWorkItemsBatch_SucceedsAsync()
        {
            int? id1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Close 1" });
            int? id2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Close 2" });
            Assert.True(id1.HasValue && id2.HasValue);
            _createdWorkItemIds.Add(id1!.Value);
            _createdWorkItemIds.Add(id2!.Value);


            try
            {
                IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.CloseWorkItemsBatchAsync(new[] { id1.Value, id2.Value });
                Assert.Equal(2, responses.Count);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
        }

        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseAndLinkDuplicatesBatch_SucceedsAsync()
        {
            int? canonical = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Canonical" });
            int? duplicate = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Duplicate" });
            Assert.True(canonical.HasValue && duplicate.HasValue);
            _createdWorkItemIds.Add(canonical!.Value);
            _createdWorkItemIds.Add(duplicate!.Value);

            var pairs = new List<(int, int)> { (duplicate.Value, canonical.Value) };

            try
            {
                IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.CloseAndLinkDuplicatesBatchAsync(pairs);
                Assert.Single(responses);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
        }

        /// <summary>
        /// TODO: correct PAT user so doesn't raise exception about permissions.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddChildWorkItemsBatch_SucceedsAsync()
        {
            int? parent = await _workItemsClient.CreateEpicAsync(new WorkItemCreateOptions { Title = "Parent Epic" });
            Assert.True(parent.HasValue);
            _createdWorkItemIds.Add(parent!.Value);

            WorkItem? epic = await _workItemsClient.GetWorkItemAsync(parent.Value);
            if(epic?.Id == null)
            {
                Assert.Fail("Failed to retrieve created Epic for child work items.");
                return;
            }

            string defaultWorkItemType = "Feature";

            var children = new List<WorkItemCreateOptions>
            {
                new WorkItemCreateOptions
                {
                    Title = "Child 1",
                    AreaPath = epic.Fields["System.AreaPath"].ToString()
                },
                new WorkItemCreateOptions
                {
                    Title = "Child 2",
                    AreaPath = epic.Fields["System.AreaPath"].ToString()
                }
            };

            try
            {
                List<WorkItem?> created = await _workItemsClient.AddChildWorkItemsBatchAsync(parent.Value, defaultWorkItemType, children);
                foreach(WorkItem? item in created)
                {
                    if(item?.Id != null)
                    {
                        _createdWorkItemIds.Add(item.Id.Value);
                    }
                }

                Assert.Equal(2, created.Count);
            }
            catch(VssServiceException ex)
            {

                Assert.Contains("TF237111: The current user does not have permissions to save work items under the specified area path.", ex.Message);
            }
        }

        [Fact]
        public async Task GetWorkItemsBatchByIds_SucceedsAsync()
        {
            int? item1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "BatchGet 1" });
            int? item2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "BatchGet 2" });
            Assert.True(item1.HasValue && item2.HasValue);
            _createdWorkItemIds.Add(item1!.Value);
            _createdWorkItemIds.Add(item2!.Value);

            IReadOnlyList<WorkItem> items = await _workItemsClient.GetWorkItemsBatchByIdsAsync(new[] { item1.Value, item2.Value });
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task CreateWorkItem_SucceedsAsync()
        {
            var fields = new List<WorkItemFieldValue>
            {
                new WorkItemFieldValue { Name = "System.Title", Value = "Arbitrary" }
            };

            WorkItem? workItem = await _workItemsClient.CreateWorkItemAsync("Task", fields);
            Assert.NotNull(workItem);
            if(workItem?.Id != null)
            {
                _createdWorkItemIds.Add(workItem.Id.Value);
            }
        }

        [Fact]
        public async Task UpdateWorkItem_SucceedsAsync()
        {
            int? itemId = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "Update Arbitrary" });
            Assert.True(itemId.HasValue);
            _createdWorkItemIds.Add(itemId!.Value);

            var updates = new List<Core.Boards.Options.WorkItemFieldUpdate>
            {
                new() { Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add, Path = "/fields/System.Title", Value = "Updated" }
            };

            WorkItem? updated = await _workItemsClient.UpdateWorkItemAsync(itemId.Value, updates);
            Assert.NotNull(updated);
        }

        [Fact]
        public async Task GetWorkItemType_SucceedsAsync()
        {
            WorkItemType type = await _workItemsClient.GetWorkItemTypeAsync(_azureDevOpsConfiguration.ProjectName, "Task");
            Assert.NotNull(type);
        }

        [Fact]
        public async Task GetQuery_SucceedsAsync()
        {
            QueryHierarchyItem query = await _workItemsClient.GetQueryAsync(_azureDevOpsConfiguration.ProjectName, "Shared Queries");
            Assert.NotNull(query);
        }

        [Fact]
        public async Task GetQueryResultsById_SucceedsAsync()
        {

            string queryName = "My Test Query";
            string wiql = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.WorkItemType] = 'Task'";
            await _workItemsClient.CreateSharedQueryAsync(_azureDevOpsConfiguration.ProjectName, queryName, wiql);

            QueryHierarchyItem root = await _workItemsClient.GetQueryAsync(
                _azureDevOpsConfiguration.ProjectName,
                "Shared Queries",
                depth: 2); // Increase depth to capture nested items

            QueryHierarchyItem? queryItem = FindFirstQuery(root);

            Assert.NotNull(queryItem); // Fail fast if no valid query is found

            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
            WorkItemQueryResult result = await _workItemsClient.GetQueryResultsByIdAsync(queryItem!.Id, teamContext);

            Assert.NotNull(result);

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

        /// <summary>
        /// TODO: correct using api instead of nuget with deprecated api version 5.0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkWorkItemsByNameBatch_SucceedsAsync()
        {
            int? w1 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "LinkName1" });
            int? w2 = await _workItemsClient.CreateTaskAsync(new WorkItemCreateOptions { Title = "LinkName2" });
            Assert.True(w1.HasValue && w2.HasValue);
            _createdWorkItemIds.Add(w1!.Value);
            _createdWorkItemIds.Add(w2!.Value);

            var links = new List<(int, int, string, string?)>
            {
                (w1.Value, w2.Value, "related", "link")
            };


            try
            {
                IReadOnlyList<WitBatchResponse> resp = await _workItemsClient.LinkWorkItemsByNameBatchAsync(links);
                Assert.Single(resp);
            }
            catch(VssServiceException ex)
            {
                Assert.Contains("Not Found", ex.Message);
            }
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
        }
    }
}
