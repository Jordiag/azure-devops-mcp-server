using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Mcp.Server.Security;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools
{
    /// <summary>
    /// Exposes work item operations from <see cref="Boards"/>
    /// through Model Context Protocol with comprehensive security validation.
    /// </summary>
    [McpServerToolType()]
    public class BoardsTools(IWorkItemsClient workItemsClient, ILogger<BoardsTools> logger, IServiceProvider serviceProvider)
    {
        private readonly IWorkItemsClient _workItemsClient = workItemsClient;
        private readonly ILogger<BoardsTools> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [McpServerTool, Description("Creates a new Epic work item in Azure DevOps. Epics represent large bodies of work that can be broken down into features and stories. Accepts title, description, tags, and other work item fields. Returns the unique work item ID of the created epic.")]
        public async Task<int> CreateEpicAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            // Validate security permissions
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, cancellationToken: cancellationToken);
            
            // Validate and sanitize input
            ValidateAndSanitizeWorkItemOptions(options);
            
            return (await _workItemsClient.CreateEpicAsync(options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new Feature work item in Azure DevOps. Features represent deliverable functionality that provides value to users and can contain multiple user stories. Accepts title, description, tags, and other work item fields. Returns the unique work item ID of the created feature.")]
        public async Task<int> CreateFeatureAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, cancellationToken: cancellationToken);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.CreateFeatureAsync(options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new User Story work item in Azure DevOps. User stories describe functionality from the user's perspective and are typically implemented within a single sprint. Accepts title, description, tags, acceptance criteria, and other work item fields. Returns the unique work item ID of the created user story.")]
        public async Task<int> CreateUserStoryAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, cancellationToken: cancellationToken);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.CreateUserStoryAsync(options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Creates a new Task work item in Azure DevOps. Tasks represent specific work activities that need to be completed, often breaking down user stories into actionable items. Accepts title, description, tags, estimated effort, and other work item fields. Returns the unique work item ID of the created task.")]
        public async Task<int> CreateTaskAsync(WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, cancellationToken: cancellationToken);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.CreateTaskAsync(options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates an existing Epic work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the epic's work item ID and the updated field values. Returns the work item ID of the updated epic.")]
        public async Task<int> UpdateEpicAsync(int epicId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, epicId.ToString(), cancellationToken);
            ValidateWorkItemId(epicId);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.UpdateEpicAsync(epicId, options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates an existing Feature work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, or custom fields. Requires the feature's work item ID and the updated field values. Returns the work item ID of the updated feature.")]
        public async Task<int> UpdateFeatureAsync(int featureId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, featureId.ToString(), cancellationToken);
            ValidateWorkItemId(featureId);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.UpdateFeatureAsync(featureId, options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates an existing User Story work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, acceptance criteria, or custom fields. Requires the user story's work item ID and the updated field values. Returns the work item ID of the updated user story.")]
        public async Task<int> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, userStoryId.ToString(), cancellationToken);
            ValidateWorkItemId(userStoryId);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.UpdateUserStoryAsync(userStoryId, options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Updates an existing Task work item in Azure DevOps. Modifies fields such as title, description, tags, state, assigned user, remaining work, or custom fields. Requires the task's work item ID and the updated field values. Returns the work item ID of the updated task.")]
        public async Task<int> UpdateTaskAsync(int taskId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, taskId.ToString(), cancellationToken);
            ValidateWorkItemId(taskId);
            ValidateAndSanitizeWorkItemOptions(options);
            return (await _workItemsClient.UpdateTaskAsync(taskId, options, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Permanently deletes a work item from Azure DevOps by its unique identifier. This action cannot be undone. The work item and all its history, attachments, and links will be removed. Returns true if deletion was successful, false otherwise.")]
        public async Task<bool> DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.DeleteWorkItems, workItemId.ToString(), cancellationToken);
            ValidateWorkItemId(workItemId);
            return (await _workItemsClient.DeleteWorkItemAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Retrieves a complete work item from Azure DevOps by its unique identifier. Returns all fields, including title, description, state, assigned user, tags, custom fields, history, and relationships. Useful for inspecting work item details or getting current field values before updates.")]
        public async Task<WorkItem> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.ReadWorkItems, workItemId.ToString(), cancellationToken);
            ValidateWorkItemId(workItemId);
            return (await _workItemsClient.GetWorkItemAsync(workItemId, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Executes a Work Item Query Language (WIQL) query against Azure DevOps and returns matching work items. WIQL allows complex filtering and sorting of work items using SQL-like syntax. Can query by any field, state, assigned user, dates, or custom criteria. Returns a list of work items matching the query criteria.")]
        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.ReadWorkItems, cancellationToken: cancellationToken);
            
            // Validate and sanitize WIQL query
            IInputSanitizer inputSanitizer = _serviceProvider.GetRequiredService<IInputSanitizer>();
            string sanitizedWiql = inputSanitizer.SanitizeWiql(wiql);
            
            return (await _workItemsClient.QueryWorkItemsAsync(sanitizedWiql, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Determines if the current Azure DevOps organization uses a system-managed process template. System processes are predefined templates (Agile, Scrum, CMMI) that cannot be customized, while inherited processes allow customization. Returns true if using a system process, false for custom/inherited processes. Essential for understanding work item type customization capabilities.")]
        public async Task<bool> IsSystemProcessAsync(CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.ReadProject, cancellationToken: cancellationToken);
            return (await _workItemsClient.IsSystemProcessAsync(cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Executes a Work Item Query Language (WIQL) query and returns only the count of matching work items without retrieving the actual work item data. Useful for performance optimization when only the quantity is needed, such as dashboard metrics or validation checks. More efficient than QueryWorkItemsAsync when work item details are not required.")]
        public async Task<int> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.ReadWorkItems, cancellationToken: cancellationToken);
            
            IInputSanitizer inputSanitizer = _serviceProvider.GetRequiredService<IInputSanitizer>();
            string sanitizedWiql = inputSanitizer.SanitizeWiql(wiql);
            
            return (await _workItemsClient.GetWorkItemCountAsync(sanitizedWiql, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Retrieves detailed information about a specific work item type within a project, including field definitions, workflow states, and rules. Essential for understanding available fields, valid state transitions, and constraints before creating or updating work items. Returns metadata about the work item type structure and configuration.")]
        public async Task<WorkItemType> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.ReadProject, cancellationToken: cancellationToken);
            
            IInputSanitizer inputSanitizer = _serviceProvider.GetRequiredService<IInputSanitizer>();
            string sanitizedProjectName = inputSanitizer.SanitizeProjectName(projectName);
            string sanitizedTypeName = inputSanitizer.SanitizeText(workItemTypeName);
            
            return (await _workItemsClient.GetWorkItemTypeAsync(sanitizedProjectName, sanitizedTypeName, cancellationToken)).EnsureSuccess(_logger);
        }

        [McpServerTool, Description("Sets the value of a custom field on an existing work item. Custom fields extend work items with organization-specific data beyond standard fields. Accepts the work item ID, field name (including custom field references), and new value. Returns the updated work item with the modified field. Useful for tracking specialized metrics or business-specific information.")]
        public async Task<WorkItem> SetCustomFieldAsync(int workItemId, string fieldName, string value, CancellationToken cancellationToken = default)
        {
            await SecurityValidator.ValidatePermissionAsync(_serviceProvider, SecurityPermission.WriteWorkItems, workItemId.ToString(), cancellationToken);
            ValidateWorkItemId(workItemId);
            
            IInputSanitizer inputSanitizer = _serviceProvider.GetRequiredService<IInputSanitizer>();
            string sanitizedFieldName = inputSanitizer.SanitizeText(fieldName);
            string sanitizedValue = inputSanitizer.SanitizeText(value);
            
            return (await _workItemsClient.SetCustomFieldAsync(workItemId, sanitizedFieldName, sanitizedValue, cancellationToken)).EnsureSuccess(_logger);
        }

        // Private helper methods for validation and sanitization

        private void ValidateAndSanitizeWorkItemOptions(WorkItemCreateOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IInputSanitizer inputSanitizer = _serviceProvider.GetRequiredService<IInputSanitizer>();

            ValidateAndSanitizeTitle(options, inputSanitizer);
            ValidateAndSanitizeDescription(options, inputSanitizer);
            ValidateAndSanitizeTags(options, inputSanitizer);
            ValidateAndSanitizeAreaPath(options, inputSanitizer);
            ValidateAndSanitizeIterationPath(options, inputSanitizer);

            _logger.LogDebug("Work item options validated and sanitized");
        }

        private void ValidateAndSanitizeTitle(WorkItemCreateOptions options, IInputSanitizer inputSanitizer)
        {
            if (string.IsNullOrEmpty(options.Title))
                return;

            string sanitizedTitle = inputSanitizer.SanitizeText(options.Title);
            if (string.IsNullOrWhiteSpace(sanitizedTitle))
                throw new ArgumentException("Work item title cannot be empty after sanitization");
            
            if (sanitizedTitle != options.Title)
            {
                _logger.LogWarning("Work item title was sanitized");
            }
        }

        private void ValidateAndSanitizeDescription(WorkItemCreateOptions options, IInputSanitizer inputSanitizer)
        {
            if (string.IsNullOrEmpty(options.Description))
                return;

            string sanitizedDescription = inputSanitizer.SanitizeHtml(options.Description);
            if (sanitizedDescription != options.Description)
            {
                _logger.LogWarning("Work item description was sanitized");
            }
        }

        private void ValidateAndSanitizeTags(WorkItemCreateOptions options, IInputSanitizer inputSanitizer)
        {
            if (string.IsNullOrEmpty(options.Tags))
                return;

            string sanitizedTags = inputSanitizer.SanitizeText(options.Tags);
            if (sanitizedTags != options.Tags)
            {
                _logger.LogWarning("Work item tags were sanitized");
            }
        }

        private void ValidateAndSanitizeAreaPath(WorkItemCreateOptions options, IInputSanitizer inputSanitizer)
        {
            if (string.IsNullOrEmpty(options.AreaPath))
                return;

            string sanitizedAreaPath = inputSanitizer.SanitizeText(options.AreaPath);
            if (sanitizedAreaPath != options.AreaPath)
            {
                _logger.LogWarning("Area path was sanitized");
            }
        }

        private void ValidateAndSanitizeIterationPath(WorkItemCreateOptions options, IInputSanitizer inputSanitizer)
        {
            if (string.IsNullOrEmpty(options.IterationPath))
                return;

            string sanitizedIterationPath = inputSanitizer.SanitizeText(options.IterationPath);
            if (sanitizedIterationPath != options.IterationPath)
            {
                _logger.LogWarning("Iteration path was sanitized");
            }
        }

        private static void ValidateWorkItemId(int workItemId)
        {
            if (workItemId <= 0)
                throw new ArgumentException("Work item ID must be a positive integer", nameof(workItemId));
        }
    }
}
