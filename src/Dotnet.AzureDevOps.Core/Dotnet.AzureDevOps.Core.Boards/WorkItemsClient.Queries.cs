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
        /// Retrieves a complete work item by its unique identifier, including all fields, relations, and metadata.
        /// This method returns comprehensive work item information suitable for detailed inspection, editing, or reporting.
        /// The work item data includes all system and custom fields, work item links, attachments, and revision history.
        /// Use this method when you need full work item details rather than summary information.
        /// </summary>
        /// <param name="workItemId">The unique identifier of the work item to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete <see cref="WorkItem"/> object if successful,
        /// or error details if the operation fails or the work item is not found.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid or zero.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as work item not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the work item.</exception>
        public async Task<AzureDevOpsActionResult<WorkItem>> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItem item = await _workItemClient.GetWorkItemAsync(
                    id: workItemId,
                    expand: WorkItemExpand.All,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<WorkItem>.Success(item, _logger);
            }
            catch(VssServiceException ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItem>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Executes a Work Item Query Language (WIQL) query to retrieve work items matching specified criteria.
        /// WIQL allows complex filtering and sorting using SQL-like syntax to query work items by any field, state, assigned user,
        /// dates, relationships, or custom criteria. This method retrieves full work item details for all matching items.
        /// Large result sets are automatically batched for optimal performance.
        /// </summary>
        /// <param name="wiql">The WIQL query string to execute (e.g., "SELECT * FROM WorkItems WHERE [State] = 'Active'").</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of <see cref="WorkItem"/> objects matching the query,
        /// or an empty list if no work items match, or error details if the operation fails or the query is invalid.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiql is null, empty, or contains invalid syntax.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as invalid query syntax.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to execute queries or access work items.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new Wiql { Query = wiql };
                WorkItemQueryResult result = await _workItemClient.QueryByWiqlAsync(query, project: _projectName, cancellationToken: cancellationToken);

                if(result.WorkItems?.Any() == true)
                {
                    int[] ids = [.. result.WorkItems.Select(w => w.Id)];
                    const int BatchSize = 200;

                    var allItems = new List<WorkItem>();

                    foreach(int[] batch in ids.Chunk(BatchSize))
                    {
                        List<WorkItem> batchItems = await _workItemClient.GetWorkItemsAsync(batch, cancellationToken: cancellationToken);
                        allItems.AddRange(batchItems);
                    }

                    return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Success(allItems, _logger);
                }

                IReadOnlyList<WorkItem> empty = Array.Empty<WorkItem>();
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Success(empty, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Executes a WIQL query and returns only the count of matching work items without retrieving the actual work item data.
        /// This method is optimized for performance when only the quantity of matching work items is needed,
        /// such as for dashboard metrics, validation checks, or pagination calculations.
        /// More efficient than QueryWorkItemsAsync when work item details are not required.
        /// </summary>
        /// <param name="wiql">The WIQL query string to execute for counting (e.g., "SELECT * FROM WorkItems WHERE [State] = 'Active'").</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the count of work items matching the query if successful,
        /// or error details if the operation fails or the query is invalid.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiql is null, empty, or contains invalid syntax.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as invalid query syntax.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to execute queries.</exception>
        public async Task<AzureDevOpsActionResult<int>> GetWorkItemCountAsync(string wiql, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<IReadOnlyList<WorkItem>> itemsResult = await QueryWorkItemsAsync(wiql, cancellationToken);
            if(!itemsResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<int>.Failure(itemsResult.ErrorMessage!, _logger);
            }

            int count = itemsResult.Value.Count;
            return AzureDevOpsActionResult<int>.Success(count, _logger);
        }

        /// <summary>
        /// Retrieves detailed information about a specific work item type within a project.
        /// This includes field definitions, workflow states, rules, and constraints for the work item type.
        /// Essential for understanding available fields, valid state transitions, and business rules
        /// before creating or updating work items of this type. Different projects may have customized work item types.
        /// </summary>
        /// <param name="projectName">The name of the project containing the work item type.</param>
        /// <param name="workItemTypeName">The name of the work item type to retrieve (e.g., "Bug", "User Story", "Task").</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="WorkItemType"/> definition if successful,
        /// or error details if the operation fails or the work item type is not found.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName or workItemTypeName is null or empty.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as work item type not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access project metadata.</exception>
        public async Task<AzureDevOpsActionResult<WorkItemType>> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemType type = await _workItemClient.GetWorkItemTypeAsync(projectName, workItemTypeName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItemType>.Success(type, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemType>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves detailed information about a work item query by its identifier or path.
        /// Queries can be personal or shared, and contain WIQL statements for filtering work items.
        /// This method returns query metadata including name, WIQL text, folder structure, and permissions.
        /// Useful for understanding existing queries, preparing to modify them, or building query management tools.
        /// </summary>
        /// <param name="projectName">The name of the project containing the query.</param>
        /// <param name="queryIdOrPath">The unique identifier (GUID) or folder path of the query to retrieve.</param>
        /// <param name="expand">Optional expansion options to include additional query details (default: null).</param>
        /// <param name="depth">The depth of child items to include in the hierarchy (default: 0 for no children).</param>
        /// <param name="includeDeleted">Whether to include deleted queries in the results (default: false).</param>
        /// <param name="useIsoDateFormat">Whether to use ISO date format in the response (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="QueryHierarchyItem"/> with query information if successful,
        /// or error details if the operation fails or the query is not found.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName or queryIdOrPath is null or empty.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as query not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the query.</exception>
        public async Task<AzureDevOpsActionResult<QueryHierarchyItem>> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default)
        {
            try
            {
                QueryHierarchyItem result = await _workItemClient.GetQueryAsync(projectName, queryIdOrPath, expand, depth, includeDeleted, useIsoDateFormat, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<QueryHierarchyItem>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<QueryHierarchyItem>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Executes a saved work item query by its unique identifier and returns the query results.
        /// This method provides a way to run predefined queries without knowing their WIQL text,
        /// using query objects that have been previously created and saved in Azure DevOps.
        /// Results include work item references and can be configured with various formatting options.
        /// </summary>
        /// <param name="queryId">The unique identifier (GUID) of the saved query to execute.</param>
        /// <param name="teamContext">The team context specifying the project and team scope for the query.</param>
        /// <param name="timePrecision">Whether to include time precision in date fields (default: false).</param>
        /// <param name="top">Maximum number of work items to return in the results (default: 50).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="WorkItemQueryResult"/> with work item references if successful,
        /// or error details if the operation fails or the query is not found.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when queryId is empty or teamContext is invalid.</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps service returns an error, such as query not found or invalid team context.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to execute the query or access the team.</exception>
        public async Task<AzureDevOpsActionResult<WorkItemQueryResult>> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemQueryResult result = await _workItemClient.QueryByIdAsync(teamContext, queryId, timePrecision, top, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WorkItemQueryResult>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WorkItemQueryResult>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Creates a shared work item query that can be used by team members to filter and view work items.
        /// Shared queries provide consistent views of work items across the team and can be reused for reporting,
        /// dashboards, or regular work item management. The query is created in the "Shared Queries" folder
        /// and becomes available to all project members with appropriate permissions.
        /// </summary>
        /// <param name="projectName">The name of the project where the shared query will be created.</param>
        /// <param name="queryName">The name for the new shared query (must be unique within the shared queries folder).</param>
        /// <param name="wiql">The WIQL (Work Item Query Language) statement that defines the query logic and filtering criteria.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the query was successfully created,
        /// or error details if the operation fails, the query name already exists, or the WIQL is invalid.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName, queryName, or wiql is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create shared queries.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query name already exists or WIQL syntax is invalid.</exception>
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

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/Shared%20Queries?api-version={GlobalConstants.ApiVersion}";

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = content
                };

                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure($"Failed to create query: {response.StatusCode} - {responseBody}", _logger);
                }

                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Deletes a shared work item query from the project's "Shared Queries" folder.
        /// This permanently removes the query and makes it unavailable to all team members.
        /// Only users with appropriate permissions can delete shared queries, and care should be taken
        /// as this operation cannot be undone. Any dashboards or reports that reference this query
        /// will be affected by its removal.
        /// </summary>
        /// <param name="projectName">The name of the project containing the shared query to delete.</param>
        /// <param name="queryName">The name of the shared query to delete (must exist in the shared queries folder).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the query was successfully deleted,
        /// or error details if the operation fails or the query does not exist.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName or queryName is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete shared queries.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not exist or cannot be deleted due to dependencies.</exception>
        public async Task<AzureDevOpsActionResult<bool>> DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default)
        {
            try
            {
                string encodedPath = Uri.EscapeDataString($"Shared Queries/{queryName}");
                string requestUrl = $"{_organizationUrl}/{projectName}/_apis/wit/queries/{encodedPath}?api-version={GlobalConstants.ApiVersion}";

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure($"Failed to delete query: {response.StatusCode} - {responseBody}", _logger);
                }

                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }
    }
}