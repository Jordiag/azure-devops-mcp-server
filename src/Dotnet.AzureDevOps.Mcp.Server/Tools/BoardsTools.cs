using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.Extensions.Logging;
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
        public async Task<int> CreateEpicAsync(WorkItemCreateOptions options) =>
            (await _workItemsClient.CreateEpicAsync(options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new Feature work item in Azure DevOps. Features represent deliverable functionality that provides value to users and can contain multiple user stories. Accepts title, description, tags, and other work item fields. Returns the unique work item ID of the created feature.")]
        public async Task<int> CreateFeatureAsync(WorkItemCreateOptions options) =>
            (await _workItemsClient.CreateFeatureAsync(options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new User Story work item in Azure DevOps. User stories describe functionality from the user's perspective and are typically implemented within a single sprint. Accepts title, description, tags, acceptance criteria, and other work item fields. Returns the unique work item ID of the created user story.")]
        public async Task<int> CreateUserStoryAsync(WorkItemCreateOptions options) =>
            (await _workItemsClient.CreateUserStoryAsync(options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Creates a new Task work item in Azure DevOps. Tasks represent specific work activities that need to be completed, often breaking down user stories into actionable items. Accepts title, description, tags, estimated effort, and other work item fields. Returns the unique work item ID of the created task.")]
        public async Task<int> CreateTaskAsync(WorkItemCreateOptions options) =>
            (await _workItemsClient.CreateTaskAsync(options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Epic work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the epic's work item ID and the updated field values. Returns the work item ID of the updated epic.")]
        public async Task<int> UpdateEpicAsync(int epicId, WorkItemCreateOptions options) =>
            (await _workItemsClient.UpdateEpicAsync(epicId, options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Feature work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the feature's work item ID and the updated field values. Returns the work item ID of the updated feature.")]
        public async Task<int> UpdateFeatureAsync(int featureId, WorkItemCreateOptions options) =>
            (await _workItemsClient.UpdateFeatureAsync(featureId, options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing User Story work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, acceptance criteria, or custom fields. Requires the user story's work item ID and the updated field values. Returns the work item ID of the updated user story.")]
        public async Task<int> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions options) =>
            (await _workItemsClient.UpdateUserStoryAsync(userStoryId, options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Updates an existing Task work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, remaining work, or custom fields. Requires the task's work item ID and the updated field values. Returns the work item ID of the updated task.")]
        public async Task<int> UpdateTaskAsync(int taskId, WorkItemCreateOptions options) =>
            (await _workItemsClient.UpdateTaskAsync(taskId, options)).EnsureSuccess(_logger);

        [McpServerTool, Description("Permanently deletes a work item from Azure DevOps by its unique identifier. This action cannot be undone. The work item and all its history, attachments, and links will be removed. Returns true if deletion was successful, false otherwise.")]
        public async Task<bool> DeleteWorkItemAsync(int workItemId) =>
            (await _workItemsClient.DeleteWorkItemAsync(workItemId)).EnsureSuccess(_logger);

        [McpServerTool, Description("Retrieves a complete work item from Azure DevOps by its unique identifier. Returns all fields, including title, description, state, assigned user, tags, custom fields, history, and relationships. Useful for inspecting work item details or getting current field values before updates.")]
        public async Task<WorkItem> GetWorkItemAsync(int workItemId) =>
            (await _workItemsClient.GetWorkItemAsync(workItemId)).EnsureSuccess(_logger);

        [McpServerTool, Description("Executes a Work Item Query Language (WIQL) query against Azure DevOps and returns matching work items. WIQL allows complex filtering and sorting of work items using SQL-like syntax. Can query by any field, state, assigned user, dates, or custom criteria. Returns a list of work items matching the query criteria.")]
        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql) =>
            (await _workItemsClient.QueryWorkItemsAsync(wiql)).EnsureSuccess(_logger);
    }
}
