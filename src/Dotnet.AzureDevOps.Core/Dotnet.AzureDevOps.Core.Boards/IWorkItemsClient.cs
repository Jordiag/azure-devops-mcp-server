using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.IO;
using WorkItemFieldUpdate = Dotnet.AzureDevOps.Core.Boards.Options.WorkItemFieldUpdate;

namespace Dotnet.AzureDevOps.Core.Boards;

public interface IWorkItemsClient : IDisposable, IAsyncDisposable
{
    Task<AzureDevOpsActionResult<Guid>> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> CreateSharedQueryAsync(string projectName, string queryName, string wiql, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Board>> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Stream>> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<WorkItemType>> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IEnumerable<WorkItemComment>>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<object>> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<QueryHierarchyItem>> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<WorkItemQueryResult>> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<BoardReference>>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<TeamSettingsIteration>> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<WorkItem>> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> IsSystemProcessAsync(CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<TeamFieldValues>> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<BacklogLevelWorkItems>> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<PredefinedQuery>> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IterationWorkItems>> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<WorkItem>> SetCustomFieldAsync(int workItemId, string fieldName, string value, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> ExecuteBatchAsync(IEnumerable<WitBatchRequest> requests, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<int>>> CreateWorkItemsMultipleCallsAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> UpdateWorkItemsBatchAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsBatchAsync(IEnumerable<(int sourceId, int targetId, string relation)> links, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseWorkItemsBatchAsync(IEnumerable<int> workItemIds, string closedState = "Closed", string? closedReason = "Duplicate", bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseAndLinkDuplicatesBatchAsync(IEnumerable<(int duplicateId, int canonicalId)> pairs, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> GetWorkItemsBatchByIdsAsync(IEnumerable<int> ids, WorkItemExpand expand = WorkItemExpand.All, IEnumerable<string>? fields = null, CancellationToken cancellationToken = default);

    Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsByNameBatchAsync(IEnumerable<(int sourceId, int targetId, string type, string? comment)> links, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default);
}

