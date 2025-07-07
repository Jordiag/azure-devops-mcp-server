using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public interface IWorkItemsClient
    {
        Task<int?> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);

        Task<int?> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);

        Task<int?> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);

        Task<int?> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);

        Task<int?> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);

        Task<int?> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);

        Task<int?> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);

        Task<int?> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);

        Task DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);

        Task<WorkItem?> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
    }
}
