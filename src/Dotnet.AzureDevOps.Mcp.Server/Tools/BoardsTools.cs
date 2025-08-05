using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
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
        public static Task<AzureDevOpsActionResult<int>> CreateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateEpicAsync(options);
        }

        [McpServerTool, Description("Creates a new Feature work item.")]
        public static Task<AzureDevOpsActionResult<int>> CreateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateFeatureAsync(options);
        }

        [McpServerTool, Description("Creates a new User Story work item.")]
        public static Task<AzureDevOpsActionResult<int>> CreateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateUserStoryAsync(options);
        }

        [McpServerTool, Description("Creates a new Task work item.")]
        public static Task<AzureDevOpsActionResult<int>> CreateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateTaskAsync(options);
        }

        [McpServerTool, Description("Updates an Epic work item.")]
        public static Task<AzureDevOpsActionResult<int>> UpdateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, int epicId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateEpicAsync(epicId, options);
        }

        [McpServerTool, Description("Updates a Feature work item.")]
        public static Task<AzureDevOpsActionResult<int>> UpdateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, int featureId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateFeatureAsync(featureId, options);
        }

        [McpServerTool, Description("Updates a User Story work item.")]
        public static Task<AzureDevOpsActionResult<int>> UpdateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, int userStoryId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateUserStoryAsync(userStoryId, options);
        }

        [McpServerTool, Description("Updates a Task work item.")]
        public static Task<AzureDevOpsActionResult<int>> UpdateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, int taskId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateTaskAsync(taskId, options);
        }

        [McpServerTool, Description("Deletes a work item by its identifier.")]
        public static Task<AzureDevOpsActionResult<bool>> DeleteWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.DeleteWorkItemAsync(workItemId);
        }

        [McpServerTool, Description("Retrieves a work item by its identifier.")]
        public static Task<AzureDevOpsActionResult<WorkItem>> GetWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemAsync(workItemId);
        }

        [McpServerTool, Description("Runs a WIQL query and returns matching work items.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> QueryWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.QueryWorkItemsAsync(wiql);
        }

        [McpServerTool, Description("Adds a comment to a work item.")]
        public static Task<AzureDevOpsActionResult<bool>> AddCommentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string comment)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddCommentAsync(workItemId, projectName, comment);
        }

        [McpServerTool, Description("Retrieves comments for a work item.")]
        public static Task<AzureDevOpsActionResult<IEnumerable<WorkItemComment>>> GetCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetCommentsAsync(workItemId);
        }

        [McpServerTool, Description("Adds an attachment to a work item.")]
        public static Task<AzureDevOpsActionResult<Guid>> AddAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string filePath)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddAttachmentAsync(workItemId, filePath);
        }

        [McpServerTool, Description("Downloads a work item attachment.")]
        public static Task<AzureDevOpsActionResult<Stream>> GetAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, Guid attachmentId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetAttachmentAsync(projectName, attachmentId);
        }

        [McpServerTool, Description("Gets work item history updates.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>> GetHistoryAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetHistoryAsync(workItemId);
        }

        [McpServerTool, Description("Creates multiple work items in a batch.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<int>>> CreateWorkItemsBatchAsync(string organizationUrl, string projectName, string personalAccessToken, string workItemType, IEnumerable<WorkItemCreateOptions> items)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateWorkItemsMultipleCallsAsync(workItemType, items);
        }

        [McpServerTool, Description("Adds a relation link between two work items.")]
        public static Task<AzureDevOpsActionResult<bool>> AddLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int fromId, int toId, string linkType)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddLinkAsync(fromId, toId, linkType);
        }

        [McpServerTool, Description("Removes a relation link from a work item.")]
        public static Task<AzureDevOpsActionResult<bool>> RemoveLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string linkUrl)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.RemoveLinkAsync(workItemId, linkUrl);
        }

        [McpServerTool, Description("Lists links associated with a work item.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>>> GetLinksAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetLinksAsync(workItemId);
        }

        [McpServerTool, Description("Lists boardId columns for a team boardId.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>> ListBoardColumnsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid boardId, TeamContext teamContext)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListBoardColumnsAsync(teamContext, boardId);
        }

        [McpServerTool, Description("Lists backlog configurations for a team.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>> ListBacklogsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListBacklogsAsync(teamContext, userState);
        }

        [McpServerTool, Description("Lists work items for a backlog category.")]
        public static Task<AzureDevOpsActionResult<BacklogLevelWorkItems>> ListBacklogWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string backlogId, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListBacklogWorkItemsAsync(teamContext, backlogId, userState);
        }

        [McpServerTool, Description("Lists work items relevant to the authenticated user.")]
        public static Task<AzureDevOpsActionResult<PredefinedQuery>> ListMyWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListMyWorkItemsAsync(queryType, top, includeCompleted, userState);
        }

        [McpServerTool, Description("Links a work item to a pull request.")]
        public static Task<AzureDevOpsActionResult<bool>> LinkWorkItemToPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string projectId, string repositoryId, int pullRequestId, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.LinkWorkItemToPullRequestAsync(projectId, repositoryId, pullRequestId, workItemId);
        }

        [McpServerTool, Description("Lists work items for a specific iteration.")]
        public static Task<AzureDevOpsActionResult<IterationWorkItems>> GetWorkItemsForIterationAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, Guid iterationId, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemsForIterationAsync(teamContext, iterationId, userState);
        }

        [McpServerTool, Description("Lists boards for a team.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<BoardReference>>> ListBoardsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListBoardsAsync(teamContext, userState);
        }

        [McpServerTool, Description("Gets a specific team iteration.")]
        public static Task<AzureDevOpsActionResult<TeamSettingsIteration>> GetTeamIterationAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, Guid iterationId, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetTeamIterationAsync(teamContext, iterationId, userState);
        }

        [McpServerTool, Description("Lists iterations for a team with a timeframe filter.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> GetTeamIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string timeframe, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetTeamIterationsAsync(teamContext, timeframe, userState);
        }

        [McpServerTool, Description("Lists iterations for a team.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string? timeFrame = null, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListIterationsAsync(teamContext, timeFrame, userState);
        }

        [McpServerTool, Description("Lists area paths for a team.")]
        public static Task<AzureDevOpsActionResult<TeamFieldValues>> ListAreasAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListAreasAsync(teamContext);
        }


        [McpServerTool, Description("Reads a custom field value from a work item.")]
        public static Task<AzureDevOpsActionResult<object>> GetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetCustomFieldAsync(workItemId, fieldName);
        }

        [McpServerTool, Description("Sets a custom field on a work item.")]
        public static Task<AzureDevOpsActionResult<WorkItem>> SetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName, string value)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.SetCustomFieldAsync(workItemId, fieldName, value);
        }

        [McpServerTool, Description("Creates a custom field if it doesn't already exist.")]
        public static Task<AzureDevOpsActionResult<WorkItemField2>> CreateCustomFieldIfDoesntExistAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string fieldName,
            string referenceName,
            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType type,
            string? description = null,
            CancellationToken cancellationToken = default)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateCustomFieldIfDoesntExistAsync(fieldName, referenceName, type, description, cancellationToken);
        }

        [McpServerTool, Description("Exports a team boardId configuration.")]
        public static Task<AzureDevOpsActionResult<Board>> ExportBoardAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string board)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ExportBoardAsync(teamContext, board);
        }

        [McpServerTool, Description("Gets a count of work items matching a WIQL query.")]
        public static Task<AzureDevOpsActionResult<int>> GetWorkItemCountAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemCountAsync(wiql);
        }

        [McpServerTool, Description("Updates multiple work items in bulk.")]
        public static Task<AzureDevOpsActionResult<bool>> BulkUpdateWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, IEnumerable<(int id, WorkItemCreateOptions options)> updates)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.BulkUpdateWorkItemsAsync(updates);
        }

        [McpServerTool, Description("Create a new work item with arbitrary fields.")]
        public static Task<AzureDevOpsActionResult<WorkItem>> CreateWorkItemAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string workItemType,
            IEnumerable<WorkItemFieldValue> fields)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateWorkItemAsync(workItemType, fields);
        }

        [McpServerTool, Description("Update a work item with arbitrary field operations.")]
        public static Task<AzureDevOpsActionResult<WorkItem>> UpdateWorkItemAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            int workItemId,
            IEnumerable<WorkItemFieldUpdate> updates)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateWorkItemAsync(workItemId, updates);
        }

        [McpServerTool, Description("Get a work item type definition.")]
        public static Task<AzureDevOpsActionResult<WorkItemType>> GetWorkItemTypeAsync(string organizationUrl, string projectName, string personalAccessToken, string workItemType)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemTypeAsync(projectName, workItemType);
        }

        [McpServerTool, Description("Retrieve a query by ID or path.")]
        public static Task<AzureDevOpsActionResult<QueryHierarchyItem>> GetQueryAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            string query,
            QueryExpand expand,
            int depth = 0,
            bool includeDeleted = false,
            bool useIsoDateFormat = false)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetQueryAsync(projectName, query, expand, depth, includeDeleted, useIsoDateFormat);
        }

        [McpServerTool, Description("Retrieve query results by ID.")]
        public static Task<AzureDevOpsActionResult<WorkItemQueryResult>> GetQueryResultsByIdAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            Guid queryId,
            TeamContext teamContext,
            bool? timePrecision = false,
            int top = 50)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetQueryResultsByIdAsync(queryId, teamContext, timePrecision, top);
        }

        [McpServerTool, Description("Link work items together in batch using friendly relation names.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsByNameBatchAsync(
            string organizationUrl,
            string projectName,
            string personalAccessToken,
            IEnumerable<(int sourceId, int targetId, string type, string? comment)> links,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.LinkWorkItemsByNameBatchAsync(links, suppressNotifications, bypassRules);
        }

        [McpServerTool, Description("Links many work-item pairs in a single $batch call.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<(int sourceId, int targetId, string relation)> links,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.LinkWorkItemsBatchAsync(links, suppressNotifications, bypassRules);
        }

        [McpServerTool, Description("Closes multiple work items (optionally setting a reason) in one batch.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseWorkItemsBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<int> workItemIds,
            string closedState = "Closed",
            string? closedReason = "Duplicate",
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CloseWorkItemsBatchAsync(workItemIds, closedState, closedReason,
                                                   suppressNotifications, bypassRules);
        }

        [McpServerTool, Description("Closes duplicates and links them to their canonical items in one batch.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseAndLinkDuplicatesBatchAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<(int duplicateId, int canonicalId)> pairs,
            bool suppressNotifications = true,
            bool bypassRules = false)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CloseAndLinkDuplicatesBatchAsync(pairs, suppressNotifications, bypassRules);
        }

        [McpServerTool, Description("Retrieves up to 200 work items in one /workitemsbatch POST.")]
        public static Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> GetWorkItemsBatchByIdsAsync(
            string organizationUrl, string projectName, string personalAccessToken,
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemsBatchByIdsAsync(ids, expand, fields);
        }
    }
}
