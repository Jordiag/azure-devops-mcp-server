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
        private static WorkItemsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
            => new(organizationUrl, projectName, personalAccessToken, logger);

        [McpServerTool, Description("Creates a new Epic work item.")]
        public static async Task<int> CreateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .CreateEpicAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new Feature work item.")]
        public static async Task<int> CreateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .CreateFeatureAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new User Story work item.")]
        public static async Task<int> CreateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .CreateUserStoryAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Creates a new Task work item.")]
        public static async Task<int> CreateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .CreateTaskAsync(options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates an Epic work item.")]
        public static async Task<int> UpdateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, int epicId, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .UpdateEpicAsync(epicId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a Feature work item.")]
        public static async Task<int> UpdateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, int featureId, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .UpdateFeatureAsync(featureId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a User Story work item.")]
        public static async Task<int> UpdateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, int userStoryId, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .UpdateUserStoryAsync(userStoryId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Updates a Task work item.")]
        public static async Task<int> UpdateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, int taskId, WorkItemCreateOptions options, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .UpdateTaskAsync(taskId, options)).EnsureSuccess();
        }

        [McpServerTool, Description("Deletes a work item by its identifier.")]
        public static async Task<bool> DeleteWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .DeleteWorkItemAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Retrieves a work item by its identifier.")]
        public static async Task<WorkItem> GetWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .GetWorkItemAsync(workItemId)).EnsureSuccess();
        }

        [McpServerTool, Description("Runs a WIQL query and returns matching work items.")]
        public static async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql, ILogger? logger = null)
        {
            return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
                .QueryWorkItemsAsync(wiql)).EnsureSuccess();
        }

        // ... repeat for all other methods, adding ILogger? logger = null and passing it to CreateClient

    }
}