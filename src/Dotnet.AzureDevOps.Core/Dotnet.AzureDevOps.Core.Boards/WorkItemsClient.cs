using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public class WorkItemsClient : IWorkItemsClient
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly WorkItemTrackingHttpClient _workItemClient;

        public WorkItemsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public Task<int?> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync("Epic", workItemCreateOptions, cancellationToken: cancellationToken);

        public Task<int?> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync("Feature", workItemCreateOptions, cancellationToken: cancellationToken);

        public Task<int?> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync("User Story", workItemCreateOptions, cancellationToken:cancellationToken);

        public Task<int?> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync("Task", workItemCreateOptions, cancellationToken:cancellationToken);

        public Task<int?> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(epicId, updateOptions, cancellationToken);

        public Task<int?> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(featureId, updateOptions, cancellationToken);

        public Task<int?> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(userStoryId, updateOptions, cancellationToken);

        public Task<int?> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(taskId, updateOptions, cancellationToken);

        public async Task DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default) =>
            await _workItemClient.DeleteWorkItemAsync(id: workItemId, cancellationToken: cancellationToken);

        private async Task<int?> CreateWorkItemAsync(string workItemType, WorkItemCreateOptions options,bool validateOnly = false, bool bypassRules = false,
            bool suppressNotifications = false,  WorkItemExpand? expand = null, CancellationToken cancellationToken = default)
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

            return newWorkItem.Id;
        }

        private async Task<int?> UpdateWorkItemAsync(int workItemId, WorkItemCreateOptions options, CancellationToken cancellationToken = default)
        {
            JsonPatchDocument patchDocument = BuildPatchDocument(options);

            WorkItem updatedWorkItem = await _workItemClient.UpdateWorkItemAsync(
                document: patchDocument,
                id: workItemId,
                cancellationToken: cancellationToken
            );

            return updatedWorkItem.Id;
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
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{_organizationUrl}/{_projectName}/_apis/wit/workItems/{workItemCreateOptions.ParentId}",
                        attributes = new { comment = "Linking to parent" }
                    }
                });

            return patchDocument;
        }

        /// <summary>
        /// Reads an existing work item by ID. Returns null if not found or if there's an access error.
        /// </summary>
        public async Task<WorkItem?> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                // By default, retrieve all fields and relations
                return await _workItemClient.GetWorkItemAsync(
                    id: workItemId,
                    expand: WorkItemExpand.All,
                    cancellationToken: cancellationToken
                );
            }
            catch(VssServiceException)
            {
                // If the item doesn't exist or we lack permission, we return null
                return null;
            }
        }
    }
}
