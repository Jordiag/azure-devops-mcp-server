using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools
{
    /// <summary>
    /// Exposes work item operations from <see cref="Boards"/>
    /// through Model Context Protocol.
    /// </summary>
    [McpServerToolType()]
    public class BoardsTools(IWorkItemsClient workItemsClient, ILogger<BoardsTools> logger)
    {
        private readonly IWorkItemsClient _workItemsClient = workItemsClient;
        private readonly ILogger<BoardsTools> _logger = logger;

        [McpServerTool, Description("Creates a new Epic work item in Azure DevOps. Epics represent large bodies of work that can be broken down into features and stories. Accepts title, description, tags, and other work item fields. Returns the unique work item ID of the created epic.")]
        public async Task<int> CreateEpicAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateEpicAsync(options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new Feature work item in Azure DevOps. Features represent deliverable functionality that provides value to users and can contain multiple user stories. Accepts title, description, tags, and other work item fields. Returns the unique work item ID of the created feature.")]
        public async Task<int> CreateFeatureAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateFeatureAsync(options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new User Story work item in Azure DevOps. User stories describe functionality from the user's perspective and are typically implemented within a single sprint. Accepts title, description, tags, acceptance criteria, and other work item fields. Returns the unique work item ID of the created user story.")]
        public async Task<int> CreateUserStoryAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateUserStoryAsync(options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new Task work item in Azure DevOps. Tasks represent specific work activities that need to be completed, often breaking down user stories into actionable items. Accepts title, description, tags, estimated effort, and other work item fields. Returns the unique work item ID of the created task.")]
        public async Task<int> CreateTaskAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateTaskAsync(options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Epic work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the epic's work item ID and the updated field values. Returns the work item ID of the updated epic.")]
        public async Task<int> UpdateEpicAsync(int epicId, WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.UpdateEpicAsync(epicId, options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Feature work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the feature's work item ID and the updated field values. Returns the work item ID of the updated feature.")]
        public async Task<int> UpdateFeatureAsync(int featureId, WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.UpdateFeatureAsync(featureId, options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing User Story work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, acceptance criteria, or custom fields. Requires the user story's work item ID and the updated field values. Returns the work item ID of the updated user story.")]
        public async Task<int> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.UpdateUserStoryAsync(userStoryId, options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Task work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, remaining work, or custom fields. Requires the task's work item ID and the updated field values. Returns the work item ID of the updated task.")]
        public async Task<int> UpdateTaskAsync(int taskId, WorkItemCreateOptions options, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.UpdateTaskAsync(taskId, options, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Permanently deletes a work item from Azure DevOps by its unique identifier. This action cannot be undone. The work item and all its history, attachments, and links will be removed. Returns true if deletion was successful, false otherwise.")]
        public async Task<bool> DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.DeleteWorkItemAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves a complete work item from Azure DevOps by its unique identifier. Returns all fields, including title, description, state, assigned user, tags, custom fields, history, and relationships. Useful for inspecting work item details or getting current field values before updates.")]
        public async Task<WorkItem> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetWorkItemAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Executes a Work Item Query Language (WIQL) query against Azure DevOps and returns matching work items. WIQL allows complex filtering and sorting of work items using SQL-like syntax. Can query by any field, state, assigned user, dates, or custom criteria. Returns a list of work items matching the query criteria.")]
        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.QueryWorkItemsAsync(wiql, cancellationToken)).EnsureSuccess(_logger);

        // System and Process Operations

        [McpServerTool, Description("Determines if the current Azure DevOps organization uses a system-managed process template. System processes are predefined templates (Agile, Scrum, CMMI) that cannot be customized, while inherited processes allow customization. Returns true if using a system process, false for custom/inherited processes. Essential for understanding work item type customization capabilities.")]
        public async Task<bool> IsSystemProcessAsync(CancellationToken cancellationToken = default) =>
            (await _workItemsClient.IsSystemProcessAsync(cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Executes a Work Item Query Language (WIQL) query and returns only the count of matching work items without retrieving the actual work item data. Useful for performance optimization when only the quantity is needed, such as dashboard metrics or validation checks. More efficient than QueryWorkItemsAsync when work item details are not required.")]
        public async Task<int> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetWorkItemCountAsync(wiql, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves detailed information about a specific work item type within a project, including field definitions, workflow states, and rules. Essential for understanding available fields, valid state transitions, and constraints before creating or updating work items. Returns metadata about the work item type structure and configuration.")]
        public async Task<WorkItemType> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetWorkItemTypeAsync(projectName, workItemTypeName, cancellationToken)).EnsureSuccess(_logger);

        // Custom Fields Operations

        [McpServerTool, Description("Sets the value of a custom field on an existing work item. Custom fields extend work items with organization-specific data beyond standard fields. Accepts the work item ID, field name (including custom field references), and new value. Returns the updated work item with the modified field. Useful for tracking specialized metrics or business-specific information.")]
        public async Task<WorkItem> SetCustomFieldAsync(int workItemId, string fieldName, string value, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.SetCustomFieldAsync(workItemId, fieldName, value, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves the current value of a specific custom field from a work item. Custom fields contain organization-specific data beyond standard work item properties. Returns the field value as an object that may need casting to the appropriate type. Useful for reading specialized metrics, business data, or custom tracking information.")]
        public async Task<object> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetCustomFieldAsync(workItemId, fieldName, cancellationToken)).EnsureSuccess(_logger);

        // Bulk Operations

        [McpServerTool, Description("Updates multiple work items in a single operation for improved performance compared to individual updates. Accepts a collection of work item IDs paired with their update options. More efficient than multiple single-item updates when modifying many work items simultaneously. Returns true if all updates succeed, false otherwise.")]
        public async Task<bool> BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.BulkUpdateWorkItemsAsync(updates, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates multiple work items of the same type using individual API calls for each item. Useful when batch operations are not suitable or when detailed error handling per item is required. Accepts work item type and collection of creation options. Returns a list of created work item IDs in the same order as the input.")]
        public async Task<IReadOnlyList<int>> CreateWorkItemsMultipleCallsAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateWorkItemsMultipleCallsAsync(workItemType, items, cancellationToken)).EnsureSuccess(_logger);

        // Attachments Operations

        [McpServerTool, Description("Uploads a file as an attachment to an existing work item. Attachments provide supporting documentation, images, logs, or other files related to the work item. Accepts the work item ID and local file path. Returns the unique attachment identifier for future reference. Useful for adding evidence, screenshots, or detailed specifications to work items.")]
        public async Task<Guid> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.AddAttachmentAsync(workItemId, filePath, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Downloads an attachment file from Azure DevOps by its unique identifier. Attachments contain supporting files related to work items such as documents, images, or logs. Requires the project name and attachment ID. Returns a stream containing the file data that can be saved locally or processed directly.")]
        public async Task<Stream> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetAttachmentAsync(projectName, attachmentId, cancellationToken)).EnsureSuccess(_logger);

        // Comments and History Operations

        [McpServerTool, Description("Adds a comment to an existing work item for team communication and progress tracking. Comments provide context, updates, decisions, or discussion points related to the work item. Requires work item ID, project name, and comment text. Returns true if the comment was successfully added. Essential for maintaining communication history and decision tracking.")]
        public async Task<bool> AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.AddCommentAsync(workItemId, projectName, comment, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all comments associated with a work item, providing the complete communication history. Comments contain team discussions, updates, decisions, and progress notes. Returns a collection of work item comments with author, timestamp, and content information. Useful for understanding work item context and team collaboration history.")]
        public async Task<IEnumerable<WorkItemComment>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetCommentsAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves the complete change history of a work item, showing all field modifications over time. History includes field changes, state transitions, assignments, and other updates with timestamps and user information. Returns a chronological list of work item updates. Essential for audit trails, understanding work item evolution, and tracking progress over time.")]
        public async Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetHistoryAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);

        // Links and Relationships Operations

        [McpServerTool, Description("Creates a relationship link between two work items to establish dependencies, hierarchies, or associations. Links define how work items relate to each other (Parent/Child, Related, Duplicate, etc.). Accepts source work item ID, target work item ID, and link type. Returns true if the link was successfully created. Essential for organizing work hierarchies and tracking dependencies.")]
        public async Task<bool> AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.AddLinkAsync(workItemId, targetWorkItemId, linkType, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all relationship links associated with a work item, showing connections to other work items, external resources, or related artifacts. Links include Parent/Child relationships, dependencies, related items, and external references. Returns a collection of work item relations with link types and target information. Useful for understanding work item context and dependencies.")]
        public async Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetLinksAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Removes a specific relationship link from a work item by its link URL. Useful for cleaning up incorrect relationships, removing obsolete dependencies, or reorganizing work item hierarchies. Requires the work item ID and the exact URL of the link to remove. Returns true if the link was successfully removed.")]
        public async Task<bool> RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.RemoveLinkAsync(workItemId, linkUrl, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a link between a work item and a pull request to establish traceability between code changes and work items. This integration connects development work with requirements, bugs, or tasks. Accepts project ID, repository ID, pull request ID, and work item ID. Returns true if the link was successfully created. Essential for traceability between code and work.")]
        public async Task<bool> LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.LinkWorkItemToPullRequestAsync(projectId, repositoryId, pullRequestId, workItemId, cancellationToken)).EnsureSuccess(_logger);

        // Query Management Operations

        [McpServerTool, Description("Creates a shared work item query that can be used by team members to filter and view work items based on specific criteria. Shared queries provide consistent views of work items across the team and can be reused for reporting or dashboards. Accepts project name, query name, and WIQL query text. Returns true if the query was successfully created.")]
        public async Task<bool> CreateSharedQueryAsync(string projectName, string queryName, string wiql, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateSharedQueryAsync(projectName, queryName, wiql, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Permanently removes a shared work item query from the project. Shared queries that are no longer needed can be deleted to maintain a clean query hierarchy. Requires project name and query name. Returns true if the query was successfully deleted. Use with caution as deletion cannot be undone.")]
        public async Task<bool> DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.DeleteSharedQueryAsync(projectName, queryName, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves detailed information about a specific work item query by its identifier or path. Queries can be personal or shared, and contain WIQL statements for filtering work items. Returns query metadata including name, WIQL text, folder structure, and permissions. Useful for understanding existing queries or preparing to modify them.")]
        public async Task<QueryHierarchyItem> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetQueryAsync(projectName, queryIdOrPath, expand, depth, includeDeleted, useIsoDateFormat, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Executes a saved work item query by its unique identifier and returns the matching work items. Provides a way to run predefined queries without knowing their WIQL text. Accepts query ID, team context, and optional parameters for result formatting and limiting. Returns query results with work item data and metadata.")]
        public async Task<WorkItemQueryResult> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetQueryResultsByIdAsync(queryId, teamContext, timePrecision, top, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves work items assigned to the current user based on predefined query types such as 'assigned to me', 'created by me', or 'following'. Provides quick access to personally relevant work items without writing custom queries. Returns a predefined query result with work items matching the specified criteria.")]
        public async Task<PredefinedQuery> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListMyWorkItemsAsync(queryType, top, includeCompleted, userState, cancellationToken)).EnsureSuccess(_logger);

        // Board Management Operations

        [McpServerTool, Description("Retrieves all Kanban boards available to a specific team within the project. Boards provide visual management of work items through columns representing workflow states. Returns a list of board references with names and identifiers. Essential for understanding team board structure and accessing board-specific operations.")]
        public async Task<IReadOnlyList<BoardReference>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListBoardsAsync(teamContext, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves the column configuration of a specific Kanban board, including column names, work-in-progress limits, and column mappings to work item states. Board columns represent the workflow stages and help visualize work progression. Returns detailed column information for board customization and work item management.")]
        public async Task<IReadOnlyList<BoardColumn>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListBoardColumnsAsync(teamContext, board, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Exports a complete Kanban board configuration including columns, swimlanes, card fields, and styling rules. Useful for backup, migration, or analyzing board settings. Requires team context and board identifier. Returns a comprehensive board object with all configuration details that can be used for board management or replication.")]
        public async Task<Board> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ExportBoardAsync(teamContext, boardId, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all backlog levels configured for a team, including requirements, features, and epics backlogs. Backlog levels define the hierarchy of work items and their relationship to portfolio planning. Returns backlog configuration details including work item types, colors, and hierarchy relationships.")]
        public async Task<IReadOnlyList<BacklogLevelConfiguration>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListBacklogsAsync(teamContext, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all work items associated with a specific backlog level for a team. Backlogs organize work items hierarchically (epics, features, stories) and provide portfolio planning views. Returns work items at the specified backlog level with their current state and priority information.")]
        public async Task<BacklogLevelWorkItems> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListBacklogWorkItemsAsync(teamContext, backlogId, userState, cancellationToken)).EnsureSuccess(_logger);

        // Team and Area Management Operations

        [McpServerTool, Description("Retrieves the area path configuration for a specific team, showing which areas of the project the team is responsible for. Area paths organize work items by product areas, features, or organizational structure. Returns team field values indicating the team's area assignments and responsibilities.")]
        public async Task<TeamFieldValues> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListAreasAsync(teamContext, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves detailed information about a specific iteration (sprint) for a team, including start and end dates, path, and current status. Iterations organize work into time-boxed periods for planning and execution. Returns complete iteration details including timeline and team assignment information.")]
        public async Task<TeamSettingsIteration> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves iterations assigned to a team within a specified timeframe (past, current, or future). Team iterations define the sprints or time periods for work planning and execution. Returns a list of team iterations with dates, names, and assignment details for sprint planning and tracking.")]
        public async Task<IReadOnlyList<TeamSettingsIteration>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all iterations available to a team, optionally filtered by timeframe. Iterations represent sprints or time periods for organizing and planning work. Returns a comprehensive list of team iterations with scheduling information, useful for sprint planning and capacity management.")]
        public async Task<IReadOnlyList<TeamSettingsIteration>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ListIterationsAsync(teamContext, timeFrame, userState, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates new iteration nodes in the project classification structure. Iterations organize work into time-based periods like sprints or releases. Accepts project name and collection of iteration creation options including names, dates, and hierarchy. Returns the created iteration classification nodes for team assignment and work planning.")]
        public async Task<IReadOnlyList<WorkItemClassificationNode>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CreateIterationsAsync(projectName, iterations, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Assigns existing iterations to a team, enabling the team to plan and execute work within those time periods. Teams can only plan work in iterations they are assigned to. Accepts team context and collection of iteration assignment options. Returns the assigned iterations with team-specific settings and permissions.")]
        public async Task<IReadOnlyList<TeamSettingsIteration>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.AssignIterationsAsync(teamContext, iterations, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves all work items assigned to a specific iteration for a team, providing a view of work planned for that sprint or time period. Returns work items with their current state, assignment, and iteration-specific details. Essential for sprint planning, capacity management, and iteration progress tracking.")]
        public async Task<IterationWorkItems> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetWorkItemsForIterationAsync(teamContext, iterationId, userState, cancellationToken)).EnsureSuccess(_logger);

        // Batch Processing Operations

        [McpServerTool, Description("Executes multiple work item operations in a single batch request for optimal performance. Batch operations can include creates, updates, and other work item modifications combined together. Accepts a collection of batch requests with different operation types. Returns batch responses indicating success or failure for each operation.")]
        public async Task<IReadOnlyList<WitBatchResponse>> ExecuteBatchAsync(IEnumerable<WitBatchRequest> requests, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.ExecuteBatchAsync(requests, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates multiple work items in a single batch operation with advanced options for notification suppression and rule bypassing. More efficient than individual updates and provides transaction-like behavior. Accepts work item ID and update option pairs with control flags. Returns batch responses with operation results and any errors.")]
        public async Task<IReadOnlyList<WitBatchResponse>> UpdateWorkItemsBatchAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.UpdateWorkItemsBatchAsync(updates, suppressNotifications, bypassRules, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates relationship links between multiple work items in a single batch operation. Efficient for establishing many relationships simultaneously, such as linking user stories to a feature or tasks to a user story. Accepts source-target ID pairs with relation types. Returns batch responses indicating success or failure for each link operation.")]
        public async Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsBatchAsync(IEnumerable<(int sourceId, int targetId, string relation)> links, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.LinkWorkItemsBatchAsync(links, suppressNotifications, bypassRules, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Closes multiple work items in a single batch operation, setting them to a specified closed state with an optional reason. Efficient for bulk closure operations such as marking completed tasks or resolving duplicate items. Accepts work item IDs with closure state and reason parameters. Returns batch responses for each closure operation.")]
        public async Task<IReadOnlyList<WitBatchResponse>> CloseWorkItemsBatchAsync(IEnumerable<int> workItemIds, string closedState = "Closed", string? closedReason = "Duplicate", bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CloseWorkItemsBatchAsync(workItemIds, closedState, closedReason, suppressNotifications, bypassRules, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Closes duplicate work items and links them to their canonical (original) items in a single batch operation. Streamlines duplicate resolution by combining closure and linking operations. Accepts pairs of duplicate and canonical work item IDs. Returns batch responses for the combined close and link operations.")]
        public async Task<IReadOnlyList<WitBatchResponse>> CloseAndLinkDuplicatesBatchAsync(IEnumerable<(int duplicateId, int canonicalId)> pairs, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.CloseAndLinkDuplicatesBatchAsync(pairs, suppressNotifications, bypassRules, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves multiple work items by their IDs in a single batch request, with options for field expansion and filtering. More efficient than individual GetWorkItem calls when accessing multiple work items. Accepts work item IDs, expansion options, and optional field filters. Returns the requested work items with specified detail levels.")]
        public async Task<IReadOnlyList<WorkItem>> GetWorkItemsBatchByIdsAsync(IEnumerable<int> ids, WorkItemExpand expand = WorkItemExpand.All, IEnumerable<string>? fields = null, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.GetWorkItemsBatchByIdsAsync(ids, expand, fields, cancellationToken)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates relationship links between work items using link type names with optional comments in a single batch operation. Provides more descriptive linking by using human-readable link type names instead of system identifiers. Accepts source-target pairs with link type names and optional comments. Returns batch responses for each link creation.")]
        public async Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsByNameBatchAsync(IEnumerable<(int sourceId, int targetId, string type, string? comment)> links, bool suppressNotifications = true, bool bypassRules = false, CancellationToken cancellationToken = default) =>
            (await _workItemsClient.LinkWorkItemsByNameBatchAsync(links, suppressNotifications, bypassRules, cancellationToken)).EnsureSuccess(_logger);
    }
}
