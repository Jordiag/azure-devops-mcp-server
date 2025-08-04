using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class WorkItemCreationTests : BoardsIntegrationTestBase
    {
        public WorkItemCreationTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateEpic_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Integration Test Epic");
            Assert.True(epicId > 0);
        }

        [Fact]
        public async Task CreateFeature_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Feature Test");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Integration Test Feature");
            Assert.True(featureId > 0);
        }

        [Fact]
        public async Task CreateUserStory_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Story Test");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Feature for Story Test");
            int storyId = await WorkItemTestHelper.CreateUserStoryAsync(WorkItemsClient, CreatedWorkItemIds, featureId, "Integration Test Story");
            Assert.True(storyId > 0);
        }

        [Fact]
        public async Task CreateTask_SucceedsAsync()
        {
            int epicId = await WorkItemTestHelper.CreateEpicAsync(WorkItemsClient, CreatedWorkItemIds, "Epic for Task Test");
            int featureId = await WorkItemTestHelper.CreateFeatureAsync(WorkItemsClient, CreatedWorkItemIds, epicId, "Feature for Task Test");
            int storyId = await WorkItemTestHelper.CreateUserStoryAsync(WorkItemsClient, CreatedWorkItemIds, featureId, "Story for Task Test");
            int taskId = await WorkItemTestHelper.CreateTaskAsync(WorkItemsClient, CreatedWorkItemIds, storyId, "Integration Test Task");
            Assert.True(taskId > 0);
        }

        [Fact]
        public async Task CreateWorkItem_SucceedsAsync()
        {
            IList<WorkItemFieldValue> fields = new List<WorkItemFieldValue>
            {
                new WorkItemFieldValue { FieldReferenceName = "System.Title", Value = "Generic Work Item" },
                new WorkItemFieldValue { FieldReferenceName = "System.Description", Value = "Created via generic API" },
                new WorkItemFieldValue { FieldReferenceName = "System.Tags", Value = "IntegrationTest" }
            };

            WorkItem? workItem = await WorkItemsClient.CreateWorkItemAsync("Task", fields);
            Assert.NotNull(workItem);
            int workItemId = workItem!.Id!.Value;
            CreatedWorkItemIds.Add(workItemId);
        }

        [Fact]
        public async Task LinkWorkItemToPullRequest_SucceedsAsync()
        {
            WorkItemCreateOptions createOptions = new WorkItemCreateOptions
            {
                Title = "Work Item for PR Link",
                Description = "Created for PR link",
                Tags = "IntegrationTest"
            };

            int? workItemIdentifier = await WorkItemsClient.CreateTaskAsync(createOptions);
            Assert.True(workItemIdentifier.HasValue);
            CreatedWorkItemIds.Add(workItemIdentifier!.Value);

            GitRef? branch = await ReposClient.CreateBranchAsync(RepositoryName, SourceBranch, $"it-{UtcStamp()}");
            Assert.NotNull(branch);

            string sourceBranch = branch!.Name!;
            PullRequestCreateOptions pullRequestOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = RepositoryName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR for link test",
                SourceBranch = sourceBranch,
                TargetBranch = TargetBranch,
                IsDraft = false
            };

            int? pullRequestIdentifier = await ReposClient.CreatePullRequestAsync(pullRequestOptions);
            Assert.True(pullRequestIdentifier.HasValue);
            CreatedPullRequestIds.Add(pullRequestIdentifier!.Value);

            await WorkItemsClient.LinkWorkItemToPullRequestAsync(
                AzureDevOpsConfiguration.ProjectId,
                AzureDevOpsConfiguration.RepositoryId,
                pullRequestIdentifier.Value,
                workItemIdentifier.Value);
        }
    }
}
