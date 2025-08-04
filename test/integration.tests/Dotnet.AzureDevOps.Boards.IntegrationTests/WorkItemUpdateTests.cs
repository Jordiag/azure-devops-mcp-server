using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests;

[TestType(TestType.Integration)]
[Component(Component.Boards)]
public class WorkItemUpdateTests : BoardsTestBase
{
    public WorkItemUpdateTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task UpdateEpic_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic to Update",
            Description = "Original description",
            Tags = "IntegrationTest",
        });

        int? updatedId = await _workItemsClient.UpdateEpicAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Epic Updated Title",
            Description = "Updated description",
            State = "Active",
            Tags = "IntegrationTest;Updated",
        });

        Assert.True(updatedId.HasValue);
    }

    [Fact]
    public async Task UpdateFeature_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic for Feature Update",
            Description = "Parent epic",
            Tags = "IntegrationTest",
        });
        int featureId = await _workItemHelper.CreateFeatureAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Feature to Update",
            Description = "Original feature",
            Tags = "IntegrationTest",
        });

        int? updatedId = await _workItemsClient.UpdateFeatureAsync(featureId, new WorkItemCreateOptions
        {
            Title = "Feature Updated Title",
            Description = "Feature now updated",
            State = "Active",
            Tags = "IntegrationTest;Updated",
        });

        Assert.True(updatedId.HasValue);
    }

    [Fact]
    public async Task UpdateUserStory_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic for Story Update",
            Description = "Parent epic",
            Tags = "IntegrationTest",
        });
        int featureId = await _workItemHelper.CreateFeatureAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Feature for Story Update",
            Description = "Parent feature",
            Tags = "IntegrationTest",
        });
        int storyId = await _workItemHelper.CreateUserStoryAsync(featureId, new WorkItemCreateOptions
        {
            Title = "Story to Update",
            Description = "Original story description",
            Tags = "IntegrationTest",
        });

        int? updatedId = await _workItemsClient.UpdateUserStoryAsync(storyId, new WorkItemCreateOptions
        {
            Title = "Story Updated Title",
            Description = "Story has been updated",
            State = "Active",
            Tags = "IntegrationTest;Updated",
        });

        Assert.True(updatedId.HasValue);
    }

    [Fact]
    public async Task UpdateTask_SucceedsAsync()
    {
        WorkItemHierarchyIds ids = await _workItemHelper.CreateWorkItemHierarchyAsync();

        int? updatedId = await _workItemsClient.UpdateTaskAsync(ids.TaskId, new WorkItemCreateOptions
        {
            Title = "Task Updated Title",
            Description = "Updated task description",
            State = "Active",
            Tags = "IntegrationTest;Updated",
        });

        Assert.True(updatedId.HasValue);
    }
}
