using System;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        /// <summary>
        /// Reads an existing work item by ID.
        /// </summary>
        public async Task<AzureDevOpsActionResult<WorkItem>> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItem item = await _workItemClient.GetWorkItemAsync(
                    id: workItemId,
                    expand: WorkItemExpand.All,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<WorkItem>.Success(item);
            }
            catch(VssServiceException ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
        {
            try
            {
                Wiql query = new Wiql { Query = wiql };
                WorkItemQueryResult result = await _workItemClient.QueryByWiqlAsync(query, project: _projectName, cancellationToken: cancellationToken);

                if(result.WorkItems?.Any() == true)
                {
                    int[] ids = [.. result.WorkItems.Select(w => w.Id)];
                    const int BatchSize = 200;

                    List<WorkItem> allItems = new List<WorkItem>();

                    foreach(int[] batch in ids.Chunk(BatchSize))
                    {
                        List<WorkItem> batchItems = await _workItemClient.GetWorkItemsAsync(batch, cancellationToken: cancellationToken);
                        allItems.AddRange(batchItems);
                    }

                    return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Success(allItems);
                }

                IReadOnlyList<WorkItem> empty = Array.Empty<WorkItem>();
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Success(empty);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<IReadOnlyList<WorkItem>> itemsResult = await QueryWorkItemsAsync(wiql, cancellationToken);
            if(!itemsResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<int>.Failure(itemsResult.ErrorMessage!);
            }

            int count = itemsResult.Value.Count;
            return AzureDevOpsActionResult<int>.Success(count);
        }

        public async Task<AzureDevOpsActionResult<WorkItemType>> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemType type = await _workItemClient.GetWorkItemTypeAsync(projectName, workItemTypeName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItemType>.Success(type);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemType>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<QueryHierarchyItem>> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default)
        {
            try
            {
                QueryHierarchyItem result = await _workItemClient.GetQueryAsync(projectName, queryIdOrPath, expand, depth, includeDeleted, useIsoDateFormat, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<QueryHierarchyItem>.Success(result);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<QueryHierarchyItem>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<WorkItemQueryResult>> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemQueryResult result = await _workItemClient.QueryByIdAsync(teamContext, queryId, timePrecision, top, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItemQueryResult>.Success(result);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemQueryResult>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> CreateSharedQueryAsync(string projectName, string queryName, string wiql, CancellationToken cancellationToken = default)
        {
            try
            {
                object requestBody = new
                {
                    name = queryName,
                    wiql = wiql,
                    isFolder = false
                };

                StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/Shared%20Queries?api-version={GlobalConstants.ApiVersion}";

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = content
                };

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure($"Failed to create query: {response.StatusCode} - {responseBody}");
                }

                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default)
        {
            try
            {
                string encodedPath = Uri.EscapeDataString($"Shared Queries/{queryName}");
                string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/{encodedPath}?api-version={GlobalConstants.ApiVersion}";

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure($"Failed to delete query: {response.StatusCode} - {responseBody}");
                }

                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }
    }
}

