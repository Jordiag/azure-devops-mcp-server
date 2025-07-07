using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools
{
    /// <summary>
    /// Exposes work item operations from <see cref="Boards"/>
    /// through Model Context Protocol.
    /// </summary>
    [McpServerToolType()]
    public static class BoardsTools
    {
        private static WorkItemsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
            => new(organizationUrl, projectName, personalAccessToken);

        [McpServerTool, Description("Creates a new Epic work item.")]
        public static Task<int?> CreateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateEpicAsync(options);
        }

        [McpServerTool, Description("Creates a new Feature work item.")]
        public static Task<int?> CreateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateFeatureAsync(options);
        }

        [McpServerTool, Description("Creates a new User Story work item.")]
        public static Task<int?> CreateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateUserStoryAsync(options);
        }

        [McpServerTool, Description("Creates a new Task work item.")]
        public static Task<int?> CreateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateTaskAsync(options);
        }

        [McpServerTool, Description("Updates an Epic work item.")]
        public static Task<int?> UpdateEpicAsync(string organizationUrl, string projectName, string personalAccessToken, int epicId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateEpicAsync(epicId, options);
        }

        [McpServerTool, Description("Updates a Feature work item.")]
        public static Task<int?> UpdateFeatureAsync(string organizationUrl, string projectName, string personalAccessToken, int featureId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateFeatureAsync(featureId, options);
        }

        [McpServerTool, Description("Updates a User Story work item.")]
        public static Task<int?> UpdateUserStoryAsync(string organizationUrl, string projectName, string personalAccessToken, int userStoryId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateUserStoryAsync(userStoryId, options);
        }

        [McpServerTool, Description("Updates a Task work item.")]
        public static Task<int?> UpdateTaskAsync(string organizationUrl, string projectName, string personalAccessToken, int taskId, WorkItemCreateOptions options)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.UpdateTaskAsync(taskId, options);
        }

        [McpServerTool, Description("Deletes a work item by its identifier.")]
        public static Task DeleteWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.DeleteWorkItemAsync(workItemId);
        }

        [McpServerTool, Description("Retrieves a work item by its identifier.")]
        public static Task<WorkItem?> GetWorkItemAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemAsync(workItemId);
        }
    }
}
