using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests;

[TestType(TestType.Integration)]
[Component(Component.Boards)]
public class WorkItemCreationTests : BoardsTestBase
{
    public WorkItemCreationTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateEpic_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        Assert.True(epicId > 0);
    }

    [Fact]
    public async Task CreateFeature_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic for Feature Test",
            Description = "Parent epic for feature creation",
            Tags = "IntegrationTest",
        });
        int featureId = await _workItemHelper.CreateFeatureAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Integration Test Feature",
            Description = "Feature referencing epic",
            Tags = "IntegrationTest",
        });
        Assert.True(featureId > 0);
    }

    [Fact]
    public async Task CreateUserStory_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic for Story Test",
            Description = "Parent epic for story creation",
            Tags = "IntegrationTest",
        });
        int featureId = await _workItemHelper.CreateFeatureAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Feature for Story Test",
            Description = "Parent feature for story creation",
            Tags = "IntegrationTest",
        });
        int storyId = await _workItemHelper.CreateUserStoryAsync(featureId, new WorkItemCreateOptions
        {
            Title = "Integration Test Story",
            Description = "Story referencing feature",
            Tags = "IntegrationTest",
        });
        Assert.True(storyId > 0);
    }

    [Fact]
    public async Task CreateTask_SucceedsAsync()
    {
        WorkItemHierarchyIds ids = await _workItemHelper.CreateWorkItemHierarchyAsync();
        Assert.True(ids.TaskId > 0);
    }
}
