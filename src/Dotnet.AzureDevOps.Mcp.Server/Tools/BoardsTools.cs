using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
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
    public class BoardsTools
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

        [McpServerTool, Description("Runs a WIQL query and returns matching work items.")]
        public static Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.QueryWorkItemsAsync(wiql);
        }

        [McpServerTool, Description("Adds a comment to a work item.")]
        public static Task AddCommentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string comment)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddCommentAsync(workItemId, projectName, comment);
        }

        [McpServerTool, Description("Retrieves comments for a work item.")]
        public static Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetCommentsAsync(workItemId);
        }

        [McpServerTool, Description("Adds an attachment to a work item.")]
        public static Task<Guid?> AddAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string filePath)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddAttachmentAsync(workItemId, filePath);
        }

        [McpServerTool, Description("Downloads a work item attachment.")]
        public static Task<Stream?> GetAttachmentAsync(string organizationUrl, string projectName, string personalAccessToken, Guid attachmentId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetAttachmentAsync(projectName, attachmentId);
        }

        [McpServerTool, Description("Gets work item history updates.")]
        public static Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetHistoryAsync(workItemId);
        }

        [McpServerTool, Description("Creates multiple work items in a batch.")]
        public static Task<IReadOnlyList<int>> CreateWorkItemsBatchAsync(string organizationUrl, string projectName, string personalAccessToken, string workItemType, IEnumerable<WorkItemCreateOptions> items)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.CreateWorkItemsBatchAsync(workItemType, items);
        }

        [McpServerTool, Description("Adds a relation link between two work items.")]
        public static Task AddLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int fromId, int toId, string linkType)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.AddLinkAsync(fromId, toId, linkType);
        }

        [McpServerTool, Description("Removes a relation link from a work item.")]
        public static Task RemoveLinkAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string linkUrl)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.RemoveLinkAsync(workItemId, linkUrl);
        }

        [McpServerTool, Description("Lists links associated with a work item.")]
        public static Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetLinksAsync(workItemId);
        }

        [McpServerTool, Description("Lists boardId columns for a team boardId.")]
        public static Task<List<BoardColumn>> ListBoardColumnsAsync(string organizationUrl, string projectName, string personalAccessToken, Guid boardId, TeamContext teamContext, object? userState)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListBoardColumnsAsync(teamContext, boardId, userState);
        }

        [McpServerTool, Description("Lists iterations for a team.")]
        public static Task<List<TeamSettingsIteration>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string? timeFrame = null, object? userState = null)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListIterationsAsync(teamContext, timeFrame, userState);
        }

        [McpServerTool, Description("Lists area paths for a team.")]
        public static Task<TeamFieldValues> ListAreasAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ListAreasAsync(teamContext);
        }

        [McpServerTool, Description("Reads a custom field value from a work item.")]
        public static Task<object?> GetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetCustomFieldAsync(workItemId, fieldName);
        }

        [McpServerTool, Description("Sets a custom field on a work item.")]
        public static Task SetCustomFieldAsync(string organizationUrl, string projectName, string personalAccessToken, int workItemId, string fieldName, object value)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.SetCustomFieldAsync(workItemId, fieldName, value);
        }

        [McpServerTool, Description("Exports a team boardId configuration.")]
        public static Task<Board?> ExportBoardAsync(string organizationUrl, string projectName, string personalAccessToken, TeamContext teamContext, string board)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.ExportBoardAsync(teamContext, board);
        }

        [McpServerTool, Description("Gets a count of work items matching a WIQL query.")]
        public static Task<int> GetWorkItemCountAsync(string organizationUrl, string projectName, string personalAccessToken, string wiql)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.GetWorkItemCountAsync(wiql);
        }

        [McpServerTool, Description("Updates multiple work items in bulk.")]
        public static Task BulkUpdateWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, IEnumerable<(int id, WorkItemCreateOptions options)> updates)
        {
            WorkItemsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
            return client.BulkUpdateWorkItemsAsync(updates);
        }
    }
}
