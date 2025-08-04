using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Xunit;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests;

public class WorkItemTestUtilities
{
    private readonly WorkItemsClient _client;
    private readonly List<int> _createdIds;

    public WorkItemTestUtilities(WorkItemsClient client, List<int> createdIds)
    {
        _client = client;
        _createdIds = createdIds;
    }

    public async Task<int> CreateEpicAsync(WorkItemCreateOptions? options = null)
    {
        options ??= new WorkItemCreateOptions
        {
            Title = "Integration Test Epic",
            Description = "Epic created by integration test",
            Tags = "IntegrationTest",
        };

        int? id = await _client.CreateEpicAsync(options);
        Assert.True(id.HasValue, "Failed to create Epic. ID was null.");
        _createdIds.Add(id.Value);
        return id.Value;
    }

    public async Task<int> CreateFeatureAsync(int? epicId = null, WorkItemCreateOptions? options = null)
    {
        options ??= new WorkItemCreateOptions
        {
            Title = "Integration Test Feature",
            Description = "Feature created by integration test",
            ParentId = epicId,
            Tags = "IntegrationTest",
        };
        if (epicId.HasValue && options.ParentId == null)
        {
            options.ParentId = epicId;
        }

        int? id = await _client.CreateFeatureAsync(options);
        Assert.True(id.HasValue, "Failed to create Feature. ID was null.");
        _createdIds.Add(id.Value);
        return id.Value;
    }

    public async Task<int> CreateUserStoryAsync(int? featureId = null, WorkItemCreateOptions? options = null)
    {
        options ??= new WorkItemCreateOptions
        {
            Title = "Integration Test Story",
            Description = "Story created by integration test",
            ParentId = featureId,
            Tags = "IntegrationTest",
        };
        if (featureId.HasValue && options.ParentId == null)
        {
            options.ParentId = featureId;
        }

        int? id = await _client.CreateUserStoryAsync(options);
        Assert.True(id.HasValue, "Failed to create User Story. ID was null.");
        _createdIds.Add(id.Value);
        return id.Value;
    }

    public async Task<int> CreateTaskAsync(int? storyId = null, WorkItemCreateOptions? options = null)
    {
        options ??= new WorkItemCreateOptions
        {
            Title = "Integration Test Task",
            Description = "Task created by integration test",
            ParentId = storyId,
            Tags = "IntegrationTest",
        };
        if (storyId.HasValue && options.ParentId == null)
        {
            options.ParentId = storyId;
        }

        int? id = await _client.CreateTaskAsync(options);
        Assert.True(id.HasValue, "Failed to create Task. ID was null.");
        _createdIds.Add(id.Value);
        return id.Value;
    }

    public async Task<WorkItemHierarchyIds> CreateWorkItemHierarchyAsync()
    {
        int epicId = await CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Epic for Hierarchy",
            Description = "Parent epic for hierarchy",
            Tags = "IntegrationTest",
        });

        int featureId = await CreateFeatureAsync(epicId, new WorkItemCreateOptions
        {
            Title = "Feature for Hierarchy",
            Description = "Feature for hierarchy",
            Tags = "IntegrationTest",
        });

        int storyId = await CreateUserStoryAsync(featureId, new WorkItemCreateOptions
        {
            Title = "Story for Hierarchy",
            Description = "Story for hierarchy",
            Tags = "IntegrationTest",
        });

        int taskId = await CreateTaskAsync(storyId, new WorkItemCreateOptions
        {
            Title = "Task for Hierarchy",
            Description = "Task for hierarchy",
            Tags = "IntegrationTest",
        });

        return new WorkItemHierarchyIds(epicId, featureId, storyId, taskId);
    }
}

public record WorkItemHierarchyIds(int EpicId, int FeatureId, int StoryId, int TaskId);
