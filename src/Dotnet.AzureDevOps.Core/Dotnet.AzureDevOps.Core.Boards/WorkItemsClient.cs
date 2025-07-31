using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItemFieldUpdate = Dotnet.AzureDevOps.Core.Boards.Options.WorkItemFieldUpdate;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public class WorkItemsClient : IWorkItemsClient
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private readonly WorkHttpClient _workClient;
        private readonly HttpClient _httpClient;
        private const string _patchMethod = Constants.PatchMethod;
        private const string ContentTypeHeader = Constants.ContentTypeHeader;
        private const string JsonPatchContentType = Constants.JsonPatchContentType;

        public WorkItemsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;

            _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };

            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
            _workClient = connection.GetClient<WorkHttpClient>();
        }

        public async Task<bool> IsSystemProcessAsync(CancellationToken cancellationToken = default)
        {
            // Include process template details
            string projectUrl = $"{_organizationUrl}/_apis/projects/{_projectName}?api-version={GlobalConstants.ApiVersion}&includeCapabilities=true";
            JsonElement projectResponse = await _httpClient.GetFromJsonAsync<JsonElement>(projectUrl, cancellationToken);

            string? processId = projectResponse
                .GetProperty("capabilities")
                .GetProperty("processTemplate")
                .GetProperty("templateTypeId")
                .GetString();

            if(string.IsNullOrEmpty(processId))
            {
                throw new InvalidOperationException("Unable to determine the process ID for the project.");
            }

            string processUrl = $"{_organizationUrl}/_apis/process/processes/{processId}?api-version={GlobalConstants.ApiVersion}";
            JsonElement processResponse = await _httpClient.GetFromJsonAsync<JsonElement>(processUrl, cancellationToken);
            string? processType = processResponse.GetProperty("type").GetString();

            return string.IsNullOrEmpty(processType)
                ? throw new InvalidOperationException("Unable to determine process type for the project.")
                : processType.Equals("system", StringComparison.OrdinalIgnoreCase);
        }

        public Task<int?> CreateEpicAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Epic",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> CreateFeatureAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Feature",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> CreateUserStoryAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "User Story",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> CreateTaskAsync(WorkItemCreateOptions workItemCreateOptions, CancellationToken cancellationToken = default) =>
            CreateWorkItemAsync(
                workItemType: "Task",
                options: workItemCreateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> UpdateEpicAsync(int epicId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: epicId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> UpdateFeatureAsync(int featureId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: featureId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> UpdateUserStoryAsync(int userStoryId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: userStoryId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public Task<int?> UpdateTaskAsync(int taskId, WorkItemCreateOptions updateOptions, CancellationToken cancellationToken = default) =>
            UpdateWorkItemAsync(
                workItemId: taskId,
                options: updateOptions,
                cancellationToken: cancellationToken);

        public async Task DeleteWorkItemAsync(int workItemId, CancellationToken cancellationToken = default) =>
            await _workItemClient.DeleteWorkItemAsync(id: workItemId, cancellationToken: cancellationToken);

        private async Task<int?> CreateWorkItemAsync(string workItemType, WorkItemCreateOptions options, bool validateOnly = false, bool bypassRules = false,
            bool suppressNotifications = false, WorkItemExpand? expand = null, CancellationToken cancellationToken = default)
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

        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
        {
            var query = new Wiql { Query = wiql };
            WorkItemQueryResult result = await _workItemClient.QueryByWiqlAsync(query, project: _projectName, cancellationToken: cancellationToken);

            if(result.WorkItems?.Any() == true)
            {
                int[] ids = [.. result.WorkItems.Select(w => w.Id)];
                List<WorkItem> items = await _workItemClient.GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
                return items;
            }

            return [];
        }

        public async Task AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default)
        {
            var commentCreate = new CommentCreate { Text = comment };
            _ = await _workItemClient.AddCommentAsync(commentCreate, projectName, workItemId, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            WorkItemComments commentsResult = await _workItemClient.GetCommentsAsync(workItemId, cancellationToken: cancellationToken);
            return (IReadOnlyList<WorkItemComment>)(commentsResult.Comments ?? []);
        }

        public async Task<Guid?> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            AttachmentReference reference = await _workItemClient.CreateAttachmentAsync(fileStream, fileName: Path.GetFileName(filePath), cancellationToken: cancellationToken);

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = Constants.JsonPatchOperationPath,
                    Value = new
                    {
                        rel = "AttachedFile",
                        url = reference.Url
                    }
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
            return reference.Id;
        }

        public async Task<Stream?> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _workItemClient.GetAttachmentContentAsync(projectName, attachmentId, cancellationToken: cancellationToken);
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            List<WorkItemUpdate> updates = await _workItemClient.GetUpdatesAsync(workItemId, cancellationToken: cancellationToken);
            return updates;
        }

        public async Task<IReadOnlyList<int>> CreateWorkItemsBatchAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default)
        {
            var createdIds = new List<int>();
            foreach(WorkItemCreateOptions itemOptions in items)
            {
                int? id = await CreateWorkItemAsync(workItemType, itemOptions, cancellationToken: cancellationToken);
                if(id.HasValue)
                    createdIds.Add(id.Value);
            }

            return createdIds;
        }

        public async Task AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default)
        {
            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = Constants.JsonPatchOperationPath,
                    Value = new
                    {
                        rel = linkType,
                        url = $"{_organizationUrl}/{_projectName}/_apis/wit/workItems/{targetWorkItemId}"
                    }
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
        }

        public async Task RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default)
        {
            WorkItem? item = await GetWorkItemAsync(workItemId, cancellationToken);
            if(item?.Relations == null)
                return;

            WorkItemRelation? relation = item.Relations.FirstOrDefault(r => r.Url == linkUrl);
            if(relation == null)
                return;

            int index = item.Relations.IndexOf(relation);
            if(index < 0)
                return;

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Remove,
                    Path = $"/relations/{index}"
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            WorkItem? item = await GetWorkItemAsync(workItemId, cancellationToken);
            return (IReadOnlyList<WorkItemRelation>)(item?.Relations ?? []);
        }

        public Task<List<BoardReference>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBoardsAsync(teamContext, userState, cancellationToken);

        public Task<TeamSettingsIteration> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken);

        public Task<List<TeamSettingsIteration>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken);

        public Task<List<BoardColumn>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBoardColumnsAsync(teamContext, board.ToString(), userState, cancellationToken: cancellationToken);

        public Task<List<BacklogLevelConfiguration>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBacklogsAsync(teamContext, userState, cancellationToken);

        public Task<BacklogLevelWorkItems> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBacklogLevelWorkItemsAsync(teamContext, backlogId, userState, cancellationToken);

        public Task<PredefinedQuery> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetPredefinedQueryResultsAsync(_projectName, queryType, top, includeCompleted, userState, cancellationToken);

        public async Task LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default)
        {
            string artifactPathValue = $"{projectId}/{repositoryId}/{pullRequestId}";
            string encodedPath = Uri.EscapeDataString(artifactPathValue);
            string vstfsUrl = $"vstfs:///Git/PullRequestId/{encodedPath}";

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = Constants.JsonPatchOperationPath,
                    Value = new
                    {
                        rel = "ArtifactLink",
                        url = vstfsUrl,
                        attributes = new { name = "Pull Request" }
                    }
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
        }

        public Task<IterationWorkItems> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetIterationWorkItemsAsync(teamContext, iterationId, userState, cancellationToken);

        public Task<List<TeamSettingsIteration>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationsAsync(teamContext, timeFrame, userState, cancellationToken: cancellationToken);

        public async Task<IReadOnlyList<WorkItemClassificationNode>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectName); // Fix for CA1510
            ArgumentNullException.ThrowIfNull(iterations); // Fix for CA1510

            var created = new List<WorkItemClassificationNode>();

            foreach(IterationCreateOptions iteration in iterations)
            {
                var node = new WorkItemClassificationNode
                {
                    Name = iteration.IterationName,
                    Attributes = new Dictionary<string, object?>()
                };

                if(iteration.StartDate.HasValue)
                    node.Attributes["startDate"] = iteration.StartDate.Value;
                if(iteration.FinishDate.HasValue)
                    node.Attributes["finishDate"] = iteration.FinishDate.Value;

                WorkItemClassificationNode result = await _workItemClient.CreateOrUpdateClassificationNodeAsync(
                    postedNode: node,
                    project: projectName,
                    structureGroup: TreeStructureGroup.Iterations,
                    path: null,
                    cancellationToken: cancellationToken);

                created.Add(result);
            }

            return created;
        }

        public async Task<IReadOnlyList<TeamSettingsIteration>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(iterations);

            var assigned = new List<TeamSettingsIteration>();
            foreach(IterationAssignmentOptions iteration in iterations)
            {
                var data = new TeamSettingsIteration
                {
                    Id = iteration.Identifier,
                    Path = iteration.Path
                };

                TeamSettingsIteration result = await _workClient.PostTeamIterationAsync(data, teamContext, cancellationToken: cancellationToken);
                assigned.Add(result);
            }

            return assigned;
        }

        public Task<TeamFieldValues> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default)
            => _workClient.GetTeamFieldValuesAsync(teamContext, cancellationToken: cancellationToken);

        public async Task<object?> GetCustomFieldAsync(int workItemId, string fieldName, CancellationToken cancellationToken = default)
        {
            WorkItem? item = await GetWorkItemAsync(workItemId, cancellationToken);
            return item == null || !item.Fields.TryGetValue(fieldName, out object? value) ? null : value;
        }

        /// <summary>
        /// Sets a custom Field. You cannot add custom fields to a work item type if your project is using a system process (e.g. Agile, Scrum, or CMMI directly) so, You need to create an inherited process from the system one.
        /// </summary>
        /// <param name="workItemId"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WorkItem> SetCustomFieldAsync(int workItemId, string fieldName, string value, CancellationToken cancellationToken = default)
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

           return await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
        }

        public async Task<WorkItemField2> CreateCustomFieldIfDoesntExistAsync(string fieldName, string referenceName, Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.FieldType type, string? description = null, CancellationToken cancellationToken = default)
        {
            var field = new WorkItemField2
            {
                Name = fieldName,
                ReferenceName = referenceName,
                Type = type,                   // "string", "integer", "boolean", "dateTime", etc.
                Description = description,
                Usage = FieldUsage.WorkItem
            };
            try
            {
                return await _workItemClient.CreateWorkItemFieldAsync(field, cancellationToken: cancellationToken);
            }
            catch(VssServiceException ex)
            {
                if(ex.Message.Contains("is already", StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
                throw;
            }
        }

        public Task<Board?> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default)
            => _workClient.GetBoardAsync(teamContext, boardId, cancellationToken: cancellationToken);


        public async Task<int> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<WorkItem> items = await QueryWorkItemsAsync(wiql, cancellationToken);
            return items.Count;
        }

        public async Task BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default)
        {
            foreach((int id, WorkItemCreateOptions options) in updates)
            {
                await UpdateWorkItemAsync(id, options, cancellationToken);
            }
        }
        /// <summary>
        /// Executes an arbitrary collection of <see cref="WitBatchRequest"/> objects in one $batch call.
        /// </summary>
        public async Task<IReadOnlyList<WitBatchResponse>> ExecuteBatchAsync(
            IEnumerable<WitBatchRequest> requests,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(requests);

            var requestList = requests.ToList(); // Materialise to inspect count, content etc. if needed

            // Optional: log or debug request count
            Debug.WriteLine($"Executing batch with {requestList.Count} request(s).");

            List<WitBatchResponse> result = await _workItemClient
                .ExecuteBatchRequest(requestList, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return (IReadOnlyList<WitBatchResponse>)result;
        }


        /// <summary>
        /// Bulk‑creates many work items of the same type (e.g. hundreds of "User Story" records)
        /// using a single $batch request.
        /// </summary>
        public Task<IReadOnlyList<WitBatchResponse>> CreateWorkItemsBatchAsync(
            string workItemType,
            IEnumerable<WorkItemCreateOptions> items,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(workItemType))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(workItemType));
            ArgumentNullException.ThrowIfNull(items);

            var batch = new List<WitBatchRequest>();
            int idItem = 1;
            foreach(WorkItemCreateOptions item in items)
            {
                JsonPatchDocument patch = BuildPatchDocument(item);
                WitBatchRequest request = _workItemClient.CreateWorkItemBatchRequest(
                    id: idItem,
                    document: patch,
                    bypassRules: bypassRules,
                    suppressNotifications: suppressNotifications);

                batch.Add(request);
                idItem++;
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Bulk‑updates an arbitrary set of existing work items in one $batch call.
        /// </summary>
        public Task<IReadOnlyList<WitBatchResponse>> UpdateWorkItemsBatchAsync(
            IEnumerable<(int id, WorkItemCreateOptions options)> updates,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updates);

            var batch = new List<WitBatchRequest>();

            foreach((int id, WorkItemCreateOptions options) in updates)
            {
                JsonPatchDocument patch = BuildPatchDocument(options);

                // Hand‑craft the batch entry because some SDK builds lack a helper.
                var request = new WitBatchRequest
                {
                    Method = _patchMethod,
                    Uri = $"/_apis/wit/workitems/{id}?api-version={GlobalConstants.ApiVersion}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}",
                    Headers = new Dictionary<string, string>
                    {
                        { ContentTypeHeader, JsonPatchContentType }
                    },
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }


        /// <summary>
        /// Adds work-item links (parent-child, duplicate, related, etc.) for many items in **one**
        /// $batch request.  Each tuple describes a single edge: (sourceId, targetId, relation).
        /// </summary>
        /// <param name="links">
        ///     Edges to create.  
        ///     Examples: "System.LinkTypes.Related",
        ///               "System.LinkTypes.Hierarchy-Forward",
        ///               "System.LinkTypes.Duplicate-Forward".
        /// </param>
        public Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsBatchAsync(
            IEnumerable<(int sourceId, int targetId, string relation)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            if(links == null)
                throw new ArgumentNullException(nameof(links));

            var batch = new List<WitBatchRequest>();

            foreach((int sourceId, int targetId, string relation) in links)
            {
                if(string.IsNullOrWhiteSpace(relation))
                    throw new ArgumentException("Relation cannot be null or whitespace.", nameof(links));

                // JSON-Patch: append a new relation entry.
                var patch = new JsonPatchDocument
            {
                new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path      = "/relations/-",
                    Value     = new
                    {
                        rel = relation,
                        url = $"vstfs:///WorkItemTracking/WorkItem/{targetId}",
                        attributes = new { comment = "Linked by batch helper" }
                    }
                }
            };

                var request = new WitBatchRequest
                {
                    Method = "PATCH",
                    Uri = $"/_apis/wit/workitems/{sourceId}?api-version={GlobalConstants.ApiVersion}" +
                              $"&bypassRules={bypassRules.ToString().ToLowerInvariant()}" +
                              $"&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}",
                    Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json-patch+json" }
                },
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Closes a list of work items by setting <c>System.State</c> (and optionally
        /// <c>System.Reason</c>) in a single $batch request.
        /// </summary>
        public Task<IReadOnlyList<WitBatchResponse>> CloseWorkItemsBatchAsync(
            IEnumerable<int> workItemIds,
            string closedState = "Closed",
            string? closedReason = "Duplicate",
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workItemIds);

            var batch = new List<WitBatchRequest>();

            foreach(int id in workItemIds)
            {
                var patch = new JsonPatchDocument
                {
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path      = "/fields/System.State",
                        Value     = closedState
                    }
                };

                if(!string.IsNullOrWhiteSpace(closedReason))
                {
                    patch.Add(new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path = "/fields/System.Reason",
                        Value = closedReason
                    });
                }

                var request = new WitBatchRequest
                {
                    Method = "PATCH",
                    Uri = $"/_apis/wit/workitems/{id}?api-version={GlobalConstants.ApiVersion}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json-patch+json" }
                    },
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Convenience macro: for each tuple (<c>duplicateId</c>, <c>canonicalId</c>) this helper
        /// (1) sets the duplicate work item to Closed/Duplicate, and (2) adds a
        /// <c>System.LinkTypes.Duplicate-Forward</c> relation to the canonical item – all within
        /// one $batch request.
        /// </summary>
        public Task<IReadOnlyList<WitBatchResponse>> CloseAndLinkDuplicatesBatchAsync(
            IEnumerable<(int duplicateId, int canonicalId)> pairs,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pairs);

            var batch = new List<WitBatchRequest>();

            foreach((int duplicateId, int canonicalId) in pairs)
            {
                var patch = new JsonPatchDocument
                {
                    // Close the item
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path      = "/fields/System.State",
                        Value     = "Closed"
                    },
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path      = "/fields/System.Reason",
                        Value     = "Duplicate"
                    },
                    // Add duplicate relation
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path      = "/relations/-",
                        Value     = new
                        {
                            rel = "System.LinkTypes.Duplicate-Forward",
                            url = $"vstfs:///WorkItemTracking/WorkItem/{canonicalId}",
                            attributes = new { comment = "Marked duplicate via batch helper" }
                        }
                    }
                };

                var request = new WitBatchRequest
                {
                    Method = "PATCH",
                    Uri = $"/_apis/wit/workitems/{duplicateId}?api-version={GlobalConstants.ApiVersion}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json-patch+json" }
                    },
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }
        /// <summary>
        /// Convenience macro: creates a set of child work items **and** links them to the given parent
        /// in two back‑to‑back $batch calls.  Returns the <see cref="WorkItem"/> instances for the
        /// newly created children.
        /// </summary>
        public async Task<List<WorkItem?>> AddChildWorkItemsBatchAsync(
            int parentId,
            string childType,
            IEnumerable<WorkItemCreateOptions> children,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            // 1. Create the children (first $batch)
            IReadOnlyList<WitBatchResponse> createResponses = await CreateWorkItemsBatchAsync(
                childType, children, suppressNotifications, bypassRules, cancellationToken);

            var newChildren = createResponses
                .Select(r => JsonSerializer.Deserialize<WorkItem>(r.Body?.ToString() ?? "{}"))
                .Where(wi => wi != null)
                .ToList();

            // 2. Link them to the parent (second $batch)
            var links = new List<(int parentId, int id, string)>();
            foreach(WorkItem? w in newChildren)
            {
                if(w != null && w.Id != null)
                {
                    links.Add((parentId, w.Id.Value, "System.LinkTypes.Hierarchy-Forward"));
                }
            }

            await LinkWorkItemsBatchAsync(links, suppressNotifications, bypassRules, cancellationToken);

            return newChildren;
        }

        /// <summary>
        /// Fetches up to 200 work‑items by ID via the read‑side <c>/workitemsbatch</c> endpoint – a
        /// single POST request instead of N individual GETs.  Mirrors the TS helper
        /// <c>get_work_items_batch_by_ids</c>.
        /// </summary>
        public async Task<IReadOnlyList<WorkItem>> GetWorkItemsBatchByIdsAsync(
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            IList<int> idList = ids as IList<int> ?? ids.ToList();

            if(idList.Count == 0)
                return Array.Empty<WorkItem>();

            var request = new WorkItemBatchGetRequest
            {
                Ids = [.. idList],
                Fields = fields?.ToList(),
                Expand = expand
            };

            List<WorkItem>? batch = await _workItemClient.GetWorkItemsBatchAsync(request, cancellationToken, cancellationToken);
            return batch ?? [];
        }

        public async Task<WorkItem?> CreateWorkItemAsync(string workItemType, IEnumerable<WorkItemFieldValue> fields, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workItemType);
            ArgumentNullException.ThrowIfNull(fields);

            JsonPatchDocument patchDocument = new JsonPatchDocument();

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

            return workItem;
        }

        public async Task<WorkItem?> UpdateWorkItemAsync(int workItemId, IEnumerable<WorkItemFieldUpdate> updates, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updates);

            JsonPatchDocument patchDocument = new JsonPatchDocument();

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

            return result;
        }

        public Task<WorkItemType> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default)
            => _workItemClient.GetWorkItemTypeAsync(projectName, workItemTypeName, cancellationToken: cancellationToken);

        public Task<QueryHierarchyItem> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default)
            => _workItemClient.GetQueryAsync(projectName, queryIdOrPath, expand, depth, includeDeleted, useIsoDateFormat, cancellationToken: cancellationToken);

        public Task<WorkItemQueryResult> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default)
            => _workItemClient.QueryByIdAsync(teamContext, queryId, timePrecision, top, cancellationToken: cancellationToken);

        public async Task CreateSharedQueryAsync(string projectName, string queryName, string wiql, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                name = queryName,
                wiql = wiql,
                isFolder = false
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/Shared%20Queries?api-version={GlobalConstants.ApiVersion}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = content
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            if(!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create query: {response.StatusCode} - {responseBody}");
            }
        }

        public async Task DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default)
        {
            string encodedPath = Uri.EscapeDataString($"Shared Queries/{queryName}");
            string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/{encodedPath}?api-version={GlobalConstants.ApiVersion}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            if(!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to delete query: {response.StatusCode} - {responseBody}");
            }
        }


        public Task<IReadOnlyList<WitBatchResponse>> LinkWorkItemsByNameBatchAsync(
            IEnumerable<(int sourceId, int targetId, string type, string? comment)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            if(links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            var batch = new List<WitBatchRequest>();

            foreach((int sourceId, int targetId, string type, string? comment) in links)
            {
                string relation = GetRelationFromName(type);

                JsonPatchDocument patch = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = Constants.JsonPatchOperationPath,
                        Value = new
                        {
                            rel = relation,
                            url = $"vstfs:///WorkItemTracking/WorkItem/{targetId}",
                            attributes = new { comment = comment ?? string.Empty }
                        }
                    }
                };

                WitBatchRequest request = new WitBatchRequest
                {
                    Method = _patchMethod,
                    Uri = $"/_apis/wit/workitems/{sourceId}?api-version={GlobalConstants.ApiVersion}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}",
                    Headers = new Dictionary<string, string>
                    {
                        { ContentTypeHeader, JsonPatchContentType }
                    },
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        private static string GetRelationFromName(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "parent" => "System.LinkTypes.Hierarchy-Reverse",
                "child" => "System.LinkTypes.Hierarchy-Forward",
                "duplicate" => "System.LinkTypes.Duplicate-Forward",
                "duplicate of" => "System.LinkTypes.Duplicate-Reverse",
                "related" => "System.LinkTypes.Related",
                "successor" => "System.LinkTypes.Dependency-Forward",
                "predecessor" => "System.LinkTypes.Dependency-Reverse",
                "tested by" => "Microsoft.VSTS.Common.TestedBy-Forward",
                "tests" => "Microsoft.VSTS.Common.TestedBy-Reverse",
                "affects" => "Microsoft.VSTS.Common.Affects-Forward",
                "affected by" => "Microsoft.VSTS.Common.Affects-Reverse",
                _ => throw new ArgumentException($"Unknown link type: {name}", nameof(name))
            };
        }
    }
}
