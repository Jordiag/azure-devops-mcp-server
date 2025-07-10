using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public interface IWorkItemsClient
    {
        Task<Guid?> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default);
        Task AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default);
        Task AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default);
        Task BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default);
        Task<int?> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
        Task<int?> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
        Task<int?> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
        Task<int?> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<int>> CreateWorkItemsBatchAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default);
        Task DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
        Task<Board?> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default);
        Task<Stream?> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default);
        Task<object?> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default);
        Task<WorkItem?> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
        Task<int> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default);
        Task<TeamFieldValues> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default);
        Task<List<BoardColumn>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default);
        Task<List<TeamSettingsIteration>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);
        Task RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default);
        Task SetCustomFieldAsync(int workItemId, string fieldName, object value, CancellationToken cancellationToken = default);
        Task<int?> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
        Task<int?> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
        Task<int?> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
        Task<int?> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
    }
}
