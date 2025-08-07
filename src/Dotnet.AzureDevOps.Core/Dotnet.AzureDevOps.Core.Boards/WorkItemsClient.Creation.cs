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
        public Task<AzureDevOpsActionResult<int>> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Epic",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Feature",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "User Story",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Task",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: epicId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: featureId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: userStoryId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<AzureDevOpsActionResult<int>> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: taskId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public async Task<AzureDevOpsActionResult<bool>> DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _workItemClient.DeleteWorkItemAsync(id: workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
                    project: _projectName,
                    type: workItemType,
                    validateOnly: validateOnly,
                    bypassRules: bypassRules,
                    suppressNotifications: suppressNotifications,
                    expand: expand,
                    cancellationToken: cancellationToken
                );

                return newWorkItem.Id.HasValue
                    ? AzureDevOpsActionResult<int>.Success(newWorkItem.Id.Value, _logger)
                    : AzureDevOpsActionResult<int>.Failure("Work item creation returned null identifier.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
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
                    return AzureDevOpsActionResult<int>.Success(updatedWorkItem.Id.Value, _logger);
                }

                return AzureDevOpsActionResult<int>.Failure("Work item update returned null identifier.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
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
                        url = $"{_organizationUrl}/{_projectName}/_apis/wit/workItems/{workItemCreateOptions.ParentId}",
                        attributes = new { comment = "Linking to parent" }
                    }
                });

            return patchDocument;
        }

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
                    project: _projectName,
                    type: workItemType,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<WorkItem>.Success(workItem, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, _logger);
            }
        }

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

                return AzureDevOpsActionResult<WorkItem>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<object>> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<WorkItem> itemResult = await GetWorkItemAsync(workItemId, cancellationToken);
            if(!itemResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<object>.Failure(itemResult.ErrorMessage!, _logger);
            }

            WorkItem item = itemResult.Value;
            return item.Fields.TryGetValue(fieldName, out object? value)
                ? AzureDevOpsActionResult<object>.Success(value, _logger)
                : AzureDevOpsActionResult<object>.Failure($"Field '{fieldName}' not found in work item {workItemId}.", _logger);
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
                return AzureDevOpsActionResult<WorkItem>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, _logger);
            }
        }

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
                return AzureDevOpsActionResult<WorkItemField2>.Success(created, _logger);
            }
            catch(VssServiceException ex)
            {
                if(ex.Message.Contains("is already", StringComparison.OrdinalIgnoreCase))
                {
                    return AzureDevOpsActionResult<WorkItemField2>.Success(field, _logger);
                }

                return AzureDevOpsActionResult<WorkItemField2>.Failure(ex, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemField2>.Failure(ex, _logger);
            }
        }
    }
}