using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using ModelContextProtocol.Server;
using WorkItemFieldUpdate = Dotnet.AzureDevOps.Core.Boards.Options.WorkItemFieldUpdate;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools
{
    /// <summary>
    /// Exposes work item operations from <see cref="Boards"/>
    /// through Model Context Protocol.
    /// </summary>
    [McpServerToolType()]
    public class BoardsTools
    {
        private static WorkItemsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
            => new(organizationUrl, projectName, personalAccessToken);

        [McpServerTool, Description("Creates a new Epic work item.")]
        public static async Task<int> CreateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateEpicAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new Feature work item.")]
        public static async Task<int> CreateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateFeatureAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new User Story work item.")]
        public static async Task<int> CreateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateUserStoryAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new Task work item.")]
        public static async Task<int> CreateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateTaskAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates an Epic work item.")]
        public static async Task<int> UpdateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, int epicId, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateEpicAsync(epicId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a Feature work item.")]
        public static async Task<int> UpdateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, int featureId, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateFeatureAsync(featureId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a User Story work item.")]
        public static async Task<int> UpdateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, int userStoryId, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateUserStoryAsync(userStoryId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a Task work item.")]
        public static async Task<int> UpdateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, int taskId, WorkItemCreateOptions options)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateTaskAsync(taskId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Deletes a work item by its identifier.")]
        public static async Task<bool> DeleteWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .DeleteWorkItemAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieves a work item by its identifier.")]
        public static async Task<WorkItem> GetWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetWorkItemAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Runs a WIQL query and returns matching work items.")]
        public static async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .QueryWorkItemsAsync(wiql)).EnsureSuccess();
        }

        [McpServerTool, Description("Adds a comment to a work item.")]
        public static async Task<bool> AddCommentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string comment)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .AddCommentAsync(workItemId, projectName, comment)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieves comments for a work item.")]
        public static async Task<IEnumerable<WorkItemComment>> GetCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetCommentsAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Adds an attachment to a work item.")]
        public static async Task<Guid> AddAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string filePath)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .AddAttachmentAsync(workItemId, filePath)).EnsureSuccess();
        }

        [McpServerTool, Description("Downloads a work item attachment.")]
        public static async Task<Stream> GetAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, Guid attachmentId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetAttachmentAsync(projectName, attachmentId)).EnsureSuccess();
        }

        [McpServerTool, Description("Gets work item history updates.")]
        public static async Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetHistoryAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates multiple work items in a batch.")]
        public static async Task<IReadOnlyList<int>> CreateWorkItemsBatchAsync(string organizationUrl, string projectName, string personalAccessToken, string workItemType, IEnumerable<WorkItemCreateOptions> items)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateWorkItemsMultipleCallsAsync(workItemType, items)).EnsureSuccess();
        }

        [McpServerTool, Description("Adds a relation link between two work items.")]
        public static async Task<bool> AddLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int fromId, int toId, string linkType)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .AddLinkAsync(fromId, toId, linkType)).EnsureSuccess();
        }

        [McpServerTool, Description("Removes a relation link from a work item.")]
        public static async Task<bool> RemoveLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string linkUrl)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .RemoveLinkAsync(workItemId, linkUrl)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists links associated with a work item.")]
        public static async Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetLinksAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists boardId columns for a team boardId.")]
        public static async Task<IReadOnlyList<BoardColumn>> ListBoardColumnsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid boardId, TeamContext teamContext)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListBoardColumnsAsync(teamContext, boardId)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists backlog configurations for a team.")]
        public static async Task<IReadOnlyList<BacklogLevelConfiguration>> ListBacklogsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListBacklogsAsync(teamContext, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists work items for a backlog category.")]
        public static async Task<BacklogLevelWorkItems> ListBacklogWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string backlogId, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListBacklogWorkItemsAsync(teamContext, backlogId, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists work items relevant to the authenticated user.")]
        public static async Task<PredefinedQuery> ListMyWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListMyWorkItemsAsync(queryType, top, includeCompleted, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Links a work item to a pull request.")]
        public static async Task<bool> LinkWorkItemToPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string projectId, string repositoryId, int pullRequestId, int workItemId)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .LinkWorkItemToPullRequestAsync(projectId, repositoryId, pullRequestId, workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists work items for a specific iteration.")]
        public static async Task<IterationWorkItems> GetWorkItemsForIterationAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, Guid iterationId, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetWorkItemsForIterationAsync(teamContext, iterationId, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists boards for a team.")]
        public static async Task<IReadOnlyList<BoardReference>> ListBoardsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListBoardsAsync(teamContext, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Gets a specific team iteration.")]
        public static async Task<TeamSettingsIteration> GetTeamIterationAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, Guid iterationId, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetTeamIterationAsync(teamContext, iterationId, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists iterations for a team with a timeframe filter.")]
        public static async Task<IReadOnlyList<TeamSettingsIteration>> GetTeamIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string timeframe, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetTeamIterationsAsync(teamContext, timeframe, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists iterations for a team.")]
        public static async Task<IReadOnlyList<TeamSettingsIteration>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string? timeFrame = null, object? userState = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListIterationsAsync(teamContext, timeFrame, userState)).EnsureSuccess();
        }

        [McpServerTool, Description("Lists area paths for a team.")]
        public static async Task<TeamFieldValues> ListAreasAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ListAreasAsync(teamContext)).EnsureSuccess();
        }

        [McpServerTool, Description("Reads a custom field value from a work item.")]
        public static async Task<object> GetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetCustomFieldAsync(workItemId, fieldName)).EnsureSuccess();
        }

        [McpServerTool, Description("Sets a custom field on a work item.")]
        public static async Task<WorkItem> SetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName, string value)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .SetCustomFieldAsync(workItemId, fieldName, value)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a custom field if it doesn't already exist.")]
        public static async Task<WorkItemField2> CreateCustomFieldIfDoesntExistAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string fieldName,
            string referenceName,
            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType type,
            string? description = null,
            CancellationToken cancellationToken = default)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateCustomFieldIfDoesntExistAsync(fieldName, referenceName, type, description, cancellationToken)).EnsureSuccess();
        }

        [McpServerTool, Description("Exports a team boardId configuration.")]
        public static async Task<Board> ExportBoardAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string board)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ExportBoardAsync(teamContext, board)).EnsureSuccess();
        }

        [McpServerTool, Description("Gets a count of work items matching a WIQL query.")]
        public static async Task<int> GetWorkItemCountAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetWorkItemCountAsync(wiql)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates multiple work items in bulk.")]
        public static async Task<bool> BulkUpdateWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, IEnumerable<(int id, WorkItemCreateOptions options)> updates)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .BulkUpdateWorkItemsAsync(updates)).EnsureSuccess();
        }

        [McpServerTool, Description("Create a new work item with arbitrary fields.")]
        public static async Task<WorkItem> CreateWorkItemAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string workItemType,
            IEnumerable<WorkItemFieldValue> fields)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateWorkItemAsync(workItemType, fields)).EnsureSuccess();
        }

        [McpServerTool, Description("Update a work item with arbitrary field operations.")]
        public static async Task<WorkItem> UpdateWorkItemAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            int workItemId,
            IEnumerable<WorkItemFieldUpdate> updates)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateWorkItemAsync(workItemId, updates)).EnsureSuccess();
        }

        [McpServerTool, Description("Get a work item type definition.")]
        public static async Task<WorkItemType> GetWorkItemTypeAsync(string organizationUrl, string projectName, string personalAccessToken, string workItemType)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetWorkItemTypeAsync(projectName, workItemType)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieve a query by ID or path.")]
        public static async Task<QueryHierarchyItem> GetQueryAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string query,
            QueryExpand expand,
            int depth = 0,
            bool includeDeleted = false,
            bool useIsoDateFormat = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetQueryAsync(projectName, query, expand, depth, includeDeleted, useIsoDateFormat)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieve query results by ID.")]
        public static async Task<WorkItemQueryResult> GetQueryResultsByIdAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            Guid queryId,
            TeamContext teamContext,
            bool? timePrecision = false,
            int top = 50)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetQueryResultsByIdAsync(queryId, teamContext, timePrecision, top)).EnsureSuccess();
        }

        [McpServerTool, Description("Link work items together in batch using friendly relation names.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsByNameBatchAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            IEnumerable<(int sourceId, int targetId, string type, string? comment)> links,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .LinkWorkItemsByNameBatchAsync(links, suppressNotifications, bypassRules)).EnsureSuccess();
        }

        [McpServerTool, Description("Links many work-item pairs in a single $batch call.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<(int sourceId, int targetId, string relation)> links,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .LinkWorkItemsBatchAsync(links, suppressNotifications, bypassRules)).EnsureSuccess();
        }

        [McpServerTool, Description("Closes multiple work items (optionally setting a reason) in one batch.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> CloseWorkItemsBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<int> workItemIds,
            string closedState = "Closed",
            string? closedReason = "Duplicate",
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CloseWorkItemsBatchAsync(workItemIds, closedState, closedReason,
                                         suppressNotifications, bypassRules)).EnsureSuccess();
        }

        [McpServerTool, Description("Closes duplicates and links them to their canonical items in one batch.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> CloseAndLinkDuplicatesBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<(int duplicateId, int canonicalId)> pairs,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CloseAndLinkDuplicatesBatchAsync(pairs, suppressNotifications, bypassRules)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieves up to 200 work items in one /workitemsbatch POST.")]
        public static async Task<IReadOnlyList<WorkItem>> GetWorkItemsBatchByIdsAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .GetWorkItemsBatchByIdsAsync(ids, expand, fields)).EnsureSuccess();
        }

        [McpServerTool, Description("Assigns iterations to a team.")]
        public static async Task<IReadOnlyList<TeamSettingsIteration>> AssignIterationsAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            TeamContext teamContext,
            IEnumerable<IterationAssignmentOptions> iterations)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .AssignIterationsAsync(teamContext, iterations)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates iterations under the project.")]
        public static async Task<IReadOnlyList<WorkItemClassificationNode>> CreateIterationsAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<IterationCreateOptions> iterations)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateIterationsAsync(projectName, iterations)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a shared WIQL query.")]
        public static async Task<bool> CreateSharedQueryAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            string queryName, string wiql)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .CreateSharedQueryAsync(projectName, queryName, wiql)).EnsureSuccess();
        }

        [McpServerTool, Description("Deletes a shared WIQL query.")]
        public static async Task<bool> DeleteSharedQueryAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            string queryName)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .DeleteSharedQueryAsync(projectName, queryName)).EnsureSuccess();
        }

        [McpServerTool, Description("Executes a batch of work item requests.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> ExecuteBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<WitBatchRequest> requests)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .ExecuteBatchAsync(requests)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates multiple work items in a batch.")]
        public static async Task<IReadOnlyList<WitBatchResponse>> UpdateWorkItemsBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<(int id, WorkItemCreateOptions options)> updates,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .UpdateWorkItemsBatchAsync(updates, suppressNotifications, bypassRules)).EnsureSuccess();
        }

        [McpServerTool, Description("Checks whether the current process is system-defined.")]
        public static async Task<bool> IsSystemProcessAsync(
            string organizationUrl, string projectName, string personalAccessToken)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken)
                .IsSystemProcessAsync()).EnsureSuccess();
        }
    }
}