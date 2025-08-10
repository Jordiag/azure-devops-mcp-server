using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItemFieldUpdate = Dotnet.AzureDevOps.Core.Boards.Options.WorkItemFieldUpdate;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        /// <summary>
        /// Creates a new Epic work item in Azure DevOps.
        /// Epics represent large bodies of work that can be broken down into features and stories.
        /// They are typically used for high-level planning and tracking major initiatives or themes.
        /// Epics sit at the top of the work item hierarchy in most process templates.
        /// </summary>
        /// <param name="workItemCreateOptions">Configuration options for the epic including title, description, tags, and custom fields.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique work item ID of the created epic if successful,
        /// or error details if the operation fails or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemCreateOptions is invalid or missing required fields.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        public Task<AzureDevOpsActionResult<int>> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Epic",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Creates a new Feature work item in Azure DevOps.
        /// Features represent deliverable functionality that provides value to users and can contain multiple user stories.
        /// They bridge the gap between high-level epics and detailed user stories, enabling feature-based planning and delivery.
        /// Features are essential for organizing work into meaningful, deliverable chunks.
        /// </summary>
        /// <param name="workItemCreateOptions">Configuration options for the feature including title, description, tags, and custom fields.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique work item ID of the created feature if successful,
        /// or error details if the operation fails or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemCreateOptions is invalid or missing required fields.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        public Task<AzureDevOpsActionResult<int>> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Feature",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Creates a new User Story work item in Azure DevOps.
        /// User stories describe functionality from the user's perspective and represent the smallest deliverable units of value.
        /// They are typically implemented within a single sprint and follow the "As a [user], I want [functionality] so that [benefit]" format.
        /// User stories are the primary work items for development teams in Agile methodologies.
        /// </summary>
        /// <param name="workItemCreateOptions">Configuration options for the user story including title, description, acceptance criteria, tags, and custom fields.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique work item ID of the created user story if successful,
        /// or error details if the operation fails or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemCreateOptions is invalid or missing required fields.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        public Task<AzureDevOpsActionResult<int>> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "User Story",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Creates a new Task work item in Azure DevOps.
        /// Tasks represent specific work activities that need to be completed, often breaking down user stories into actionable items.
        /// They are the most granular level of work tracking and typically include detailed implementation steps.
        /// Tasks help teams track progress, estimate effort, and manage detailed work allocation.
        /// </summary>
        /// <param name="workItemCreateOptions">Configuration options for the task including title, description, estimated effort, remaining work, tags, and custom fields.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique work item ID of the created task if successful,
        /// or error details if the operation fails or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemCreateOptions is invalid or missing required fields.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        public Task<AzureDevOpsActionResult<int>> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Task",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Updates an existing Epic work item with new field values and properties.
        /// This method allows modification of epic details such as title, description, state, assigned user,
        /// priority, and custom fields. The epic's work item ID must be valid and accessible to the user.
        /// Changes are applied using Azure DevOps patch operations for efficient updates.
        /// </summary>
        /// <param name="epicId">The unique work item ID of the epic to update.</param>
        /// <param name="updateOptions">The field values and properties to update on the epic.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the work item ID of the updated epic if successful,
        /// or error details if the operation fails, the epic is not found, or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when epicId is invalid or updateOptions contains invalid data.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify the epic.</exception>
        public Task<AzureDevOpsActionResult<int>> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: epicId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Updates an existing Feature work item with new field values and properties.
        /// This method allows modification of feature details such as title, description, state, assigned user,
        /// priority, business value, and custom fields. The feature's work item ID must be valid and accessible to the user.
        /// Changes are applied using Azure DevOps patch operations for efficient updates.
        /// </summary>
        /// <param name="featureId">The unique work item ID of the feature to update.</param>
        /// <param name="updateOptions">The field values and properties to update on the feature.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the work item ID of the updated feature if successful,
        /// or error details if the operation fails, the feature is not found, or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when featureId is invalid or updateOptions contains invalid data.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify the feature.</exception>
        public Task<AzureDevOpsActionResult<int>> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: featureId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Updates an existing User Story work item with new field values and properties.
        /// This method allows modification of user story details such as title, description, acceptance criteria,
        /// state, assigned user, story points, priority, and custom fields. The user story's work item ID must be valid and accessible to the user.
        /// Changes are applied using Azure DevOps patch operations for efficient updates.
        /// </summary>
        /// <param name="userStoryId">The unique work item ID of the user story to update.</param>
        /// <param name="updateOptions">The field values and properties to update on the user story.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the work item ID of the updated user story if successful,
        /// or error details if the operation fails, the user story is not found, or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when userStoryId is invalid or updateOptions contains invalid data.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify the user story.</exception>
        public Task<AzureDevOpsActionResult<int>> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: userStoryId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Updates an existing Task work item with new field values and properties.
        /// This method allows modification of task details such as title, description, state, assigned user,
        /// remaining work, completed work, activity type, and custom fields. The task's work item ID must be valid and accessible to the user.
        /// Changes are applied using Azure DevOps patch operations for efficient updates.
        /// </summary>
        /// <param name="taskId">The unique work item ID of the task to update.</param>
        /// <param name="updateOptions">The field values and properties to update on the task.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the work item ID of the updated task if successful,
        /// or error details if the operation fails, the task is not found, or validation errors occur.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when taskId is invalid or updateOptions contains invalid data.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify the task.</exception>
        public Task<AzureDevOpsActionResult<int>> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: taskId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Permanently deletes a work item from Azure DevOps.
        /// This operation is irreversible and will remove the work item and all its history, attachments, and links.
        /// The deletion affects all users and systems depending on the work item. Use with extreme caution.
        /// Consider moving work items to a "Removed" state instead of deleting them to preserve audit trails.
        /// </summary>
        /// <param name="workItemId">The unique identifier of the work item to delete.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the deletion was successful,
        /// or error details if the operation fails, the work item is not found, or deletion is not allowed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid or zero.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item cannot be deleted due to business rules or dependencies.</exception>
        public async Task<AzureDevOpsActionResult<bool>> DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _workItemClient.DeleteWorkItemAsync(id: workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        private async Task<AzureDevOpsActionResult<int>> CreateWorkItemAsync(string workItemType, WorkItemCreateOptions options, bool validateOnly = false, bool bypassRules = false,
            bool suppressNotifications = false, WorkItemExpand? expand = null, CancellationToken cancellationToken = default)
        {
            try
            {
                JsonPatchDocument patchDocument = BuildPatchDocument(options);

                WorkItem newWorkItem = await _workItemClient.CreateWorkItemAsync(
                    document: patchDocument,
                    project: ProjectName,
                    type: workItemType,
                    validateOnly: validateOnly,
                    bypassRules: bypassRules,
                    suppressNotifications: suppressNotifications,
                    expand: expand,
                    cancellationToken: cancellationToken
                );

                return newWorkItem.Id.HasValue
                    ? AzureDevOpsActionResult<int>.Success(newWorkItem.Id.Value, Logger)
                    : AzureDevOpsActionResult<int>.Failure("Work item creation returned null identifier.", Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        private async Task<AzureDevOpsActionResult<int>> UpdateWorkItemAsync(int workItemId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                JsonPatchDocument patchDocument = BuildPatchDocument(options);

                WorkItem updatedWorkItem = await _workItemClient.UpdateWorkItemAsync(
                    document: patchDocument,
                    id: workItemId,
                    cancellationToken: cancellationToken
                );

                if(updatedWorkItem.Id.HasValue)
                {
                    return AzureDevOpsActionResult<int>.Success(updatedWorkItem.Id.Value, Logger);
                }

                return AzureDevOpsActionResult<int>.Failure("Work item update returned null identifier.", Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Adds a string field to the patch document if it's not null/whitespace.
        /// </summary>
        private static void AddStringField(JsonPatchDocument patchDocument, string? value, string fieldPath)
        {
            if(!string.IsNullOrWhiteSpace(value))
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = fieldPath,
                    Value = value
                });
        }

        /// <summary>
        /// Adds a numeric field to the patch document if it has a non-null value.
        /// </summary>
        private static void AddNumericField<T>(JsonPatchDocument patchDocument, T? value, string fieldPath)
            where T : struct
        {
            if(value.HasValue)
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = fieldPath,
                    Value = value.Value
                });
        }

        /// <summary>
        /// Builds a JSON Patch document from work item creation options for Azure DevOps API operations.
        /// This method converts the structured WorkItemCreateOptions into the JSON Patch format required
        /// by Azure DevOps REST API for creating and updating work items. It handles field mapping,
        /// data type conversions, and hierarchical relationships (parent links) according to Azure DevOps
        /// work item field schema and API requirements.
        /// </summary>
        /// <param name="workItemCreateOptions">The work item creation options containing field values to convert.</param>
        /// <returns>
        /// A <see cref="JsonPatchDocument"/> containing properly formatted patch operations for Azure DevOps API,
        /// including field updates and relationship links ready for work item creation or modification.
        /// </returns>
        /// <remarks>
        /// This method maps common work item fields like Title, Description, AssignedTo, State, Priority, 
        /// StoryPoints, and others to their corresponding Azure DevOps system field paths. It also handles
        /// parent-child relationships by creating appropriate link operations when ParentId is specified.
        /// </remarks>
        private JsonPatchDocument BuildPatchDocument(WorkItemCreateOptions workItemCreateOptions)
        {
            var patchDocument = new JsonPatchDocument();

            // Strings
            AddStringField(patchDocument, workItemCreateOptions.Title, "/fields/System.Title");
            AddStringField(patchDocument, workItemCreateOptions.Description, "/fields/System.Description");
            AddStringField(patchDocument, workItemCreateOptions.AssignedTo, "/fields/System.AssignedTo");
            AddStringField(patchDocument, workItemCreateOptions.State, "/fields/System.State");
            AddStringField(patchDocument, workItemCreateOptions.Tags, "/fields/System.Tags");
            AddStringField(patchDocument, workItemCreateOptions.AcceptanceCriteria, "/fields/Microsoft.VSTS.Common.AcceptanceCriteria");
            AddStringField(patchDocument, workItemCreateOptions.AreaPath, "/fields/System.AreaPath");
            AddStringField(patchDocument, workItemCreateOptions.IterationPath, "/fields/System.IterationPath");

            // Numerics
            AddNumericField(patchDocument, workItemCreateOptions.Priority, "/fields/Microsoft.VSTS.Common.Priority");
            AddNumericField(patchDocument, workItemCreateOptions.StoryPoints, "/fields/Microsoft.VSTS.Scheduling.StoryPoints");
            AddNumericField(patchDocument, workItemCreateOptions.Effort, "/fields/Microsoft.VSTS.Scheduling.Effort");
            AddNumericField(patchDocument, workItemCreateOptions.RemainingWork, "/fields/Microsoft.VSTS.Scheduling.RemainingWork");
            AddNumericField(patchDocument, workItemCreateOptions.OriginalEstimate, "/fields/Microsoft.VSTS.Scheduling.OriginalEstimate");

            // Parent link
            if(workItemCreateOptions.ParentId.HasValue)
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = Constants.JsonPatchOperationPath,
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{OrganizationUrl}/{ProjectName}/_apis/wit/workItems/{workItemCreateOptions.ParentId}",
                        attributes = new { comment = "Linking to parent" }
                    }
                });

            return patchDocument;
        }

        /// <summary>
        /// Creates a new work item with the specified type and field values using a generic approach.
        /// This method provides a flexible way to create work items by specifying individual field values
        /// rather than using predefined work item type methods. Useful for creating work items with
        /// custom fields or when the specific work item type is determined dynamically at runtime.
        /// </summary>
        /// <param name="workItemType">The type of work item to create (e.g., "Task", "Bug", "User Story", "Feature", "Epic").</param>
        /// <param name="fields">A collection of field name-value pairs to populate in the new work item.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the created work item with all populated fields,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemType is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when fields collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work item type is invalid or required fields are missing.</exception>
        public async Task<AzureDevOpsActionResult<WorkItem>> CreateWorkItemAsync(string workItemType, IEnumerable<WorkItemFieldValue> fields, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(workItemType);
                ArgumentNullException.ThrowIfNull(fields);

                var patchDocument = new JsonPatchDocument();

                foreach(WorkItemFieldValue field in fields)
                {
                    patchDocument.Add(new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{field.Name}",
                        Value = field.Value
                    });

                    if(!string.IsNullOrWhiteSpace(field.Format) && field.Format.Equals("Markdown", StringComparison.OrdinalIgnoreCase) && field.Value.Length > 50)
                    {
                        patchDocument.Add(new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = $"/multilineFieldsFormat/{field.Name}",
                            Value = field.Format
                        });
                    }
                }

                WorkItem workItem = await _workItemClient.CreateWorkItemAsync(
                    document: patchDocument,
                    project: ProjectName,
                    type: workItemType,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<WorkItem>.Success(workItem, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Updates an existing work item with the specified field changes using a generic approach.
        /// This method provides a flexible way to update work items by specifying individual field updates
        /// rather than using predefined work item type methods. Each update can specify the operation type
        /// (add, replace, remove), making it suitable for complex field modifications including custom fields.
        /// </summary>
        /// <param name="workItemId">The ID of the work item to update.</param>
        /// <param name="updates">A collection of field updates specifying which fields to modify and how.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the updated work item with all current field values,
        /// or error details if the update fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when updates collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to update work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist or field updates are invalid.</exception>
        public async Task<AzureDevOpsActionResult<WorkItem>> UpdateWorkItemAsync(int workItemId, IEnumerable<WorkItemFieldUpdate> updates, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(updates);

                var patchDocument = new JsonPatchDocument();

                foreach(WorkItemFieldUpdate update in updates)
                {
                    patchDocument.Add(new JsonPatchOperation
                    {
                        Operation = update.Operation,
                        Path = update.Path,
                        Value = update.Value
                    });

                    if(!string.IsNullOrWhiteSpace(update.Format) && update.Format.Equals("Markdown", StringComparison.OrdinalIgnoreCase) && update.Value != null && update.Value.Length > 50)
                    {
                        string formatPath = $"/multilineFieldsFormat{update.Path.Replace("/fields", string.Empty, StringComparison.OrdinalIgnoreCase)}";
                        patchDocument.Add(new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = formatPath,
                            Value = update.Format
                        });
                    }
                }

                WorkItem result = await _workItemClient.UpdateWorkItemAsync(
                    document: patchDocument,
                    id: workItemId,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<WorkItem>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the value of a custom field from a specific work item by field name.
        /// This method provides access to custom fields that have been added to work item types
        /// beyond the standard system fields. Useful for accessing organization-specific data
        /// or custom tracking information stored in work items.
        /// </summary>
        /// <param name="workItemId">The ID of the work item from which to retrieve the custom field value.</param>
        /// <param name="fieldName">The name or reference name of the custom field to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the custom field value,
        /// or error details if the work item or field doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid or fieldName is null/empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to read work item fields.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist or the field is not found.</exception>
        public async Task<AzureDevOpsActionResult<object>> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<WorkItem> itemResult = await GetWorkItemAsync(workItemId, cancellationToken);
            if(!itemResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<object>.Failure(itemResult.ErrorMessage!, Logger);
            }

            WorkItem item = itemResult.Value;
            return item.Fields.TryGetValue(fieldName, out object? value)
                ? AzureDevOpsActionResult<object>.Success(value, Logger)
                : AzureDevOpsActionResult<object>.Failure($"Field '{fieldName}' not found in work item {workItemId}.", Logger);
        }

        /// <summary>
        /// Sets a custom Field. You cannot add custom fields to a work item type if your project is using a system process (e.g. Agile, Scrum, or CMMI directly) so, You need to create an inherited process from the system one.
        /// </summary>
        /// <param name="workItemId"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AzureDevOpsActionResult<WorkItem>> SetCustomFieldAsync(int workItemId, string fieldName, string value, CancellationToken cancellationToken = default)
        {
            try
            {
                var patch = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Replace,
                        Path = $"/fields/{fieldName}",
                        Value = value
                    }
                };

                WorkItem result = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItem>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Creates a custom work item field if it doesn't already exist in the project.
        /// This method enables dynamic creation of custom fields for work item types, allowing
        /// organizations to extend work item tracking with their own data requirements.
        /// The field is created at the project level and becomes available for use in work items.
        /// </summary>
        /// <param name="fieldName">The display name for the custom field.</param>
        /// <param name="referenceName">The unique reference name for the field (typically follows naming conventions like "Custom.FieldName").</param>
        /// <param name="type">The data type for the custom field (String, Integer, DateTime, etc.).</param>
        /// <param name="description">Optional description explaining the purpose and usage of the custom field.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the created or existing work item field definition,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when fieldName or referenceName is null/empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create custom fields.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the field type is invalid or field creation rules are violated.</exception>
        public async Task<AzureDevOpsActionResult<WorkItemField2>> CreateCustomFieldIfDoesntExistAsync(string fieldName, string referenceName, Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType type, string? description = null, CancellationToken cancellationToken = default)
        {
            var field = new WorkItemField2
            {
                Name = fieldName,
                ReferenceName = referenceName,
                Type = type,
                Description = description,
                Usage = FieldUsage.WorkItem
            };

            try
            {
                WorkItemField2 created = await _workItemClient.CreateWorkItemFieldAsync(field, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItemField2>.Success(created, Logger);
            }
            catch(VssServiceException ex)
            {
                if(ex.Message.Contains("is already", StringComparison.OrdinalIgnoreCase))
                {
                    return AzureDevOpsActionResult<WorkItemField2>.Success(field, Logger);
                }

                return AzureDevOpsActionResult<WorkItemField2>.Failure(ex, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemField2>.Failure(ex, Logger);
            }
        }
    }
}
