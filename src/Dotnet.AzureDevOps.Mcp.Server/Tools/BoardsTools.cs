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
    public class BoardsTools
    {
        private readonly IWorkItemsClient _workItemsClient;
        private readonly ILogger<BoardsTools> _logger;

        public BoardsTools(IWorkItemsClient workItemsClient, ILogger<BoardsTools> logger)
        {
            _workItemsClient = workItemsClient;
            _logger = logger;
        }

        [McpServerTool, Description("Creates a new Epic work item.")]
        public async Task<int> CreateEpicAsync(WorkItemCreateOptions options)
        {
            return (await _workItemsClient.CreateEpicAsync(options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new Feature work item.")]
        public async Task<int> CreateFeatureAsync(WorkItemCreateOptions options)
        {
            return (await _workItemsClient.CreateFeatureAsync(options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new User Story work item.")]
        public async Task<int> CreateUserStoryAsync(WorkItemCreateOptions options)
        {
            return (await _workItemsClient.CreateUserStoryAsync(options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new Task work item.")]
        public async Task<int> CreateTaskAsync(WorkItemCreateOptions options)
        {
            return (await _workItemsClient.CreateTaskAsync(options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates an Epic work item.")]
        public async Task<int> UpdateEpicAsync(int epicId, WorkItemCreateOptions options)
        {
            return (await _workItemsClient.UpdateEpicAsync(epicId, options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates a Feature work item.")]
        public async Task<int> UpdateFeatureAsync(int featureId, WorkItemCreateOptions options)
        {
            return (await _workItemsClient.UpdateFeatureAsync(featureId, options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates a User Story work item.")]
        public async Task<int> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions options)
        {
            return (await _workItemsClient.UpdateUserStoryAsync(userStoryId, options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates a Task work item.")]
        public async Task<int> UpdateTaskAsync(int taskId, WorkItemCreateOptions options)
        {
            return (await _workItemsClient.UpdateTaskAsync(taskId, options)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Deletes a work item by its identifier.")]
        public async Task<bool> DeleteWorkItemAsync(int workItemId)
        {
            return (await _workItemsClient.DeleteWorkItemAsync(workItemId)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Retrieves a work item by its identifier.")]
        public async Task<WorkItem> GetWorkItemAsync(int workItemId)
        {
            return (await _workItemsClient.GetWorkItemAsync(workItemId)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Runs a WIQL query and returns matching work items.")]
        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql)
        {
            return (await _workItemsClient.QueryWorkItemsAsync(wiql)).EnsureSuccess(_logger);
        }
    }
}
