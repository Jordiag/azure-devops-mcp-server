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
    public partial class WorkItemsClient
    {
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

        public async Task<int> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<WorkItem> items = await QueryWorkItemsAsync(wiql, cancellationToken);
            return items.Count;
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
    }
}

