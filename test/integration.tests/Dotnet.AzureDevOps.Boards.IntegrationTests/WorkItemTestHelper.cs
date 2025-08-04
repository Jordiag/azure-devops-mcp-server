using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    internal static class WorkItemTestHelper
    {
        internal static async Task<int> CreateEpicAsync(WorkItemsClient workItemsClient, List<int> createdIds, string title)
        {
            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = title,
                Description = "Created by integration test",
                Tags = "IntegrationTest"
            };

            int? epicId = await workItemsClient.CreateEpicAsync(options);
            if(!epicId.HasValue)
            {
                throw new InvalidOperationException("Failed to create Epic. Id was null.");
            }

            createdIds.Add(epicId.Value);
            return epicId.Value;
        }

        internal static async Task<int> CreateFeatureAsync(WorkItemsClient workItemsClient, List<int> createdIds, int epicId, string title)
        {
            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = title,
                Description = "Created by integration test",
                ParentId = epicId,
                Tags = "IntegrationTest"
            };

            int? featureId = await workItemsClient.CreateFeatureAsync(options);
            if(!featureId.HasValue)
            {
                throw new InvalidOperationException("Failed to create Feature. Id was null.");
            }

            createdIds.Add(featureId.Value);
            return featureId.Value;
        }

        internal static async Task<int> CreateUserStoryAsync(WorkItemsClient workItemsClient, List<int> createdIds, int featureId, string title)
        {
            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = title,
                Description = "Created by integration test",
                ParentId = featureId,
                Tags = "IntegrationTest"
            };

            int? storyId = await workItemsClient.CreateUserStoryAsync(options);
            if(!storyId.HasValue)
            {
                throw new InvalidOperationException("Failed to create User Story. Id was null.");
            }

            createdIds.Add(storyId.Value);
            return storyId.Value;
        }

        internal static async Task<int> CreateTaskAsync(WorkItemsClient workItemsClient, List<int> createdIds, int storyId, string title)
        {
            WorkItemCreateOptions options = new WorkItemCreateOptions
            {
                Title = title,
                Description = "Created by integration test",
                ParentId = storyId,
                Tags = "IntegrationTest"
            };

            int? taskId = await workItemsClient.CreateTaskAsync(options);
            if(!taskId.HasValue)
            {
                throw new InvalidOperationException("Failed to create Task. Id was null.");
            }

            createdIds.Add(taskId.Value);
            return taskId.Value;
        }

        internal static async Task<(int EpicId, int FeatureId, int StoryId, int TaskId)> CreateWorkItemHierarchyAsync(WorkItemsClient workItemsClient, List<int> createdIds)
        {
            int epicId = await CreateEpicAsync(workItemsClient, createdIds, "Integration Test Epic");
            int featureId = await CreateFeatureAsync(workItemsClient, createdIds, epicId, "Integration Test Feature");
            int storyId = await CreateUserStoryAsync(workItemsClient, createdIds, featureId, "Integration Test Story");
            int taskId = await CreateTaskAsync(workItemsClient, createdIds, storyId, "Integration Test Task");
            return (epicId, featureId, storyId, taskId);
        }
    }
}
