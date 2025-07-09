using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public class WorkItemsClient : IWorkItemsClient
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private readonly WorkHttpClient _workClient;
        private readonly TeamHttpClient _teamClient;
        private readonly string _personalAccessToken;

        public WorkItemsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;
            _personalAccessToken = personalAccessToken;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
            _workClient = connection.GetClient<WorkHttpClient>();
            _teamClient = connection.GetClient<TeamHttpClient>();
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

        public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
        {
            var query = new Wiql { Query = wiql };
            WorkItemQueryResult result = await _workItemClient.QueryByWiqlAsync(query, project: _projectName, cancellationToken: cancellationToken);
            if(result.WorkItems != null)
            {
                if(result.WorkItems.Count() > 0)
                {
                    int[] ids = [.. result.WorkItems.Select(w => w.Id)];
                    List<WorkItem> items = await _workItemClient.GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
                    return items;
                }

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
                    Path = "/relations/-",
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
            foreach(WorkItemCreateOptions opt in items)
            {
                int? id = await CreateWorkItemAsync(workItemType, opt, cancellationToken: cancellationToken);
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
                    Path = "/relations/-",
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

            int index = (item.Relations as List<WorkItemRelation>)?.FindIndex(r => r.Url == linkUrl) ?? -1;
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

        public Task<List<BoardColumn>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBoardColumnsAsync(teamContext, board.ToString(), userState, cancellationToken: cancellationToken);

        public Task<List<TeamSettingsIteration>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationsAsync(teamContext, timeFrame, userState, cancellationToken: cancellationToken);

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
        public async Task SetCustomFieldAsync(int workItemId, string fieldName, object value, CancellationToken cancellationToken = default)
        {
            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{fieldName}",
                    Value = value
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
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

        public async Task<bool> CreateTeamAsync(string teamName, string teamDescription)
        {
            var newTeam = new WebApiTeam()
            {
                Name = teamName,
                Description = teamDescription
            };

            try
            {
                WebApiTeam createdTeam = await _teamClient.CreateTeamAsync(newTeam, _projectName);
                return createdTeam.Description == teamDescription && createdTeam.Name == teamName;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<Guid> GetTeamIdAsync(string teamName)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);
                return team.Id;
            }
            catch(Exception)
            {
                return Guid.Empty;
            }
        }

        public async Task<bool> UpdateTeamDescriptionAsync(string teamName, string newDescription)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);

                var updatedTeam = new WebApiTeam()
                {
                    Description = newDescription
                };

                WebApiTeam webApiTeam = await _teamClient.UpdateTeamAsync(updatedTeam, _projectName, team.Id.ToString());

                return webApiTeam.Description == newDescription && webApiTeam.Name == teamName;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<List<BoardReference>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default) =>
            await _workClient.GetBoardsAsync(teamContext, userState, cancellationToken);

        public async Task<TeamSettingsIteration> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default) =>
            await _workClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken);

        public async Task<List<TeamSettingsIteration>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default) =>
            await _workClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken);

        /// <summary>
        /// Deletes a team from the specified project in the organization. Doesn't work, Gives a 500 so far!!!
        /// </summary>
        /// <remarks>This method sends an HTTP DELETE request to the organization's API to remove the
        /// specified team. Ensure that the provided <paramref name="teamGuid"/> corresponds to an existing
        /// team.</remarks>
        /// <param name="teamGuid">The unique identifier of the team to delete.</param>
        /// <returns><see langword="true"/> if the team was successfully deleted; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DeleteTeamAsync(Guid teamGuid)
        {
            try
            {
                using var client = new HttpClient();
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                string url = $"{_organizationUrl}/_apis/projects/{_projectName}/teams/{teamGuid}?api-version={Constants.ApiVersion}";

                HttpResponseMessage response = await client.DeleteAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreateInheritedProcessAsync(
            string newProcessName,
            string description,
            string baseProcessName // e.g. "Agile", "Scrum", "CMMI"
            )
        {
            // Map base process names to internal process IDs
            string parentProcessId = baseProcessName.ToLower() switch
            {
                "agile" => "adcc42ab-9882-485e-a3ed-7678f01f66bc",
                "scrum" => "6b724908-ef14-45cf-84f8-768b5384da45",
                "cmmi" => "27450541-8e31-4150-9947-dc59f998fc01",
                _ => throw new ArgumentException("Unsupported base process name")
            };

            string url = $"{_organizationUrl}/_apis/work/processadmin/processes/inherit?api-version={Constants.ApiVersion}";

            using var client = new HttpClient();

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                name = newProcessName,
                description = description,
                parentProcessTypeId = parentProcessId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Inherited process '{newProcessName}' created successfully.");
                return true;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to create process: {response.StatusCode}\n{error}");
                return false;
            }
        }

        public async Task<bool> DeleteInheritedProcessAsync(string organization, string personalAccessToken, string processId)
        {
            string url = $"https://dev.azure.com/{organization}/_apis/work/processadmin/processes/{processId}?api-version=7.1-preview.1";

            using var client = new HttpClient();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            HttpResponseMessage response = await client.DeleteAsync(url);

            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Process deleted successfully.");
                return true;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to delete process: {response.StatusCode}\n{error}");
                return false;
            }
        }

    }
}
