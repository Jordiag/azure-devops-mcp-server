using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        private const string CreateWorkItemsMultipleCallsOperation = "CreateWorkItemsMultipleCalls";
        private const string BulkUpdateWorkItemsOperation = "BulkUpdateWorkItems";
        private const string ExecuteBatchOperation = "ExecuteBatch";
        private const string UpdateWorkItemsBatchOperation = "UpdateWorkItemsBatch";
        private const string LinkWorkItemsBatchOperation = "LinkWorkItemsBatch";
        private const string CloseWorkItemsBatchOperation = "CloseWorkItemsBatch";
        private const string CloseAndLinkDuplicatesBatchOperation = "CloseAndLinkDuplicatesBatch";
        private const string GetWorkItemsBatchByIdsOperation = "GetWorkItemsBatchByIds";
        private const string LinkWorkItemsByNameBatchOperation = "LinkWorkItemsByNameBatch";

        private const string DefaultClosedState = "Closed";
        private const string DefaultClosedReason = "Duplicate";
        private const string DefaultLinkComment = "Linked by batch helper";
        private const string DuplicateLinkComment = "Marked duplicate via batch helper";
        private const string DuplicateLinkType = "System.LinkTypes.Duplicate-Forward";

        private const string SystemStateFieldPath = "/fields/System.State";
        private const string SystemReasonFieldPath = "/fields/System.Reason";
        private const string RelationsPath = "/relations/-";

        /// <summary>
        /// Creates multiple work items of the same type using individual API calls for each item.
        /// This method processes each work item creation sequentially, which provides better error isolation
        /// but may be slower than batch operations. Use this when you need granular control over creation
        /// or when different work items may have varying success requirements.
        /// </summary>
        /// <param name="workItemType">The type of work items to create (e.g., "Task", "Bug", "User Story").</param>
        /// <param name="items">A collection of work item creation options defining the fields and values for each work item.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of IDs for successfully created work items,
        /// or error details if the operation fails. Note that some items may succeed while others fail.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemType is null or empty, or items collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when API requests fail or return error status codes.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work item type is invalid or required fields are missing.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<int>>> CreateWorkItemsMultipleCallsAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<int> createdIds = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var ids = new List<int>();
                    foreach(WorkItemCreateOptions itemOptions in items)
                    {
                        AzureDevOpsActionResult<int> id = await CreateWorkItemAsync(workItemType, itemOptions, cancellationToken: cancellationToken);
                        if(id.IsSuccessful)
                        {
                            ids.Add(id.Value);
                        }
                    }
                    return (IReadOnlyList<int>)ids;
                }, CreateWorkItemsMultipleCallsOperation, OperationType.Create);

                return CreateSuccessResult(createdIds);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<int>>(ex);
            }
        }

        /// <summary>
        /// Updates multiple work items using individual API calls for each update operation.
        /// This method processes each work item update sequentially, allowing for different update
        /// options per work item but potentially slower performance than true batch operations.
        /// Each work item is updated independently, so partial success is possible.
        /// </summary>
        /// <param name="updates">A collection of tuples containing work item IDs and their corresponding update options.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if all updates were successful,
        /// or error details if any operation fails. Note that some updates may succeed while others fail.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when updates collection is null or contains invalid work item IDs.</exception>
        /// <exception cref="HttpRequestException">Thrown when API requests fail or return error status codes.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to update work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or required fields are invalid.</exception>
        public async Task<AzureDevOpsActionResult<bool>> BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default)
        {
            try
            {
                bool result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    foreach((int id, WorkItemCreateOptions options) in updates)
                    {
                        await UpdateWorkItemAsync(id, options, cancellationToken);
                    }
                    return true;
                }, BulkUpdateWorkItemsOperation, OperationType.Update);

                return CreateSuccessResult(result);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<bool>(ex);
            }
        }

        /// <summary>
        /// Executes a batch of work item operations using Azure DevOps batch API for optimal performance.
        /// This method processes multiple work item operations (create, update, delete) in a single API call,
        /// significantly reducing network overhead and improving performance when working with multiple items.
        /// Batch operations are atomic per item but not across the entire batch.
        /// </summary>
        /// <param name="requests">A collection of work item batch requests defining the operations to perform.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each request,
        /// or error details if the batch operation fails. Individual operations within the batch may have different success states.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when requests collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission for batch operations.</exception>
        /// <exception cref="InvalidOperationException">Thrown when batch request format is invalid or exceeds limits.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> ExecuteBatchAsync(IEnumerable<WitBatchRequest> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(requests);
                    var requestList = requests.ToList();

                    List<WitBatchResponse> result = await _workItemClient
                        .ExecuteBatchRequest(requestList, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return (IReadOnlyList<WitBatchResponse>)result;
                }, ExecuteBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        /// <summary>
        /// Updates multiple work items using Azure DevOps batch API for optimal performance.
        /// This method creates a batch request containing multiple PATCH operations and executes them
        /// in a single API call, providing significant performance benefits over individual updates.
        /// All work items are updated with their respective field changes as specified in the options.
        /// </summary>
        /// <param name="updates">A collection of tuples containing work item IDs and their corresponding update options.</param>
        /// <param name="suppressNotifications">Whether to suppress email notifications for these updates (default: true).</param>
        /// <param name="bypassRules">Whether to bypass work item rules during updates (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each update,
        /// or error details if the batch operation fails. Individual updates may succeed or fail independently.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when updates collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to update work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or field updates are invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> UpdateWorkItemsBatchAsync(
            IEnumerable<(int id, WorkItemCreateOptions options)> updates,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(updates);

                    var batch = updates.Select(update =>
                        CreateWorkItemUpdateRequest(update.id, update.options, suppressNotifications, bypassRules))
                        .ToList();

                    AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> inner = await ExecuteBatchAsync(batch, cancellationToken);
                    return inner.IsSuccessful ? inner.Value : [];
                }, UpdateWorkItemsBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        /// <summary>
        /// Creates links between multiple work items using Azure DevOps batch API for optimal performance.
        /// This method establishes relationships between work items (such as parent-child, related, duplicate, etc.)
        /// in a single batch operation. Work item links help organize work hierarchically and track dependencies
        /// across different work items in the project.
        /// </summary>
        /// <param name="links">A collection of tuples defining the source work item ID, target work item ID, and relationship type.</param>
        /// <param name="suppressNotifications">Whether to suppress email notifications for these link operations (default: true).</param>
        /// <param name="bypassRules">Whether to bypass work item rules during link creation (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each link operation,
        /// or error details if the batch operation fails. Individual link operations may succeed or fail independently.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when links collection is null.</exception>
        /// <exception cref="ArgumentException">Thrown when relation type is null or whitespace for any link.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to link work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or relationship type is invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsBatchAsync(
            IEnumerable<(int sourceId, int targetId, string relation)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(links);

                    var batch = links.Select(link =>
                    {
                        ValidateRelation(link.relation);
                        return CreateLinkRequest(link.sourceId, link.targetId, link.relation, DefaultLinkComment, suppressNotifications, bypassRules);
                    }).ToList();

                    AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> inner = await ExecuteBatchAsync(batch, cancellationToken);
                    return inner.IsSuccessful ? inner.Value : [];
                }, LinkWorkItemsBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        /// <summary>
        /// Closes multiple work items using Azure DevOps batch API for optimal performance.
        /// This method updates the state of multiple work items to a closed state (typically "Closed" or "Done")
        /// and optionally sets a closure reason. This is useful for bulk operations such as closing duplicate
        /// work items or completing multiple tasks simultaneously.
        /// </summary>
        /// <param name="workItemIds">A collection of work item IDs to close.</param>
        /// <param name="closedState">The state to set for closed work items (default: "Closed").</param>
        /// <param name="closedReason">Optional reason for closing the work items (default: "Duplicate").</param>
        /// <param name="suppressNotifications">Whether to suppress email notifications for these state changes (default: true).</param>
        /// <param name="bypassRules">Whether to bypass work item rules during state changes (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each closure operation,
        /// or error details if the batch operation fails. Individual closure operations may succeed or fail independently.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when workItemIds collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to close work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or state transitions are invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseWorkItemsBatchAsync(
            IEnumerable<int> workItemIds,
            string closedState = DefaultClosedState,
            string? closedReason = DefaultClosedReason,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(workItemIds);

                    var batch = workItemIds.Select(id =>
                        CreateCloseWorkItemRequest(id, closedState, closedReason))
                        .ToList();

                    AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> inner = await ExecuteBatchAsync(batch, cancellationToken);
                    return inner.IsSuccessful ? inner.Value : [];
                }, CloseWorkItemsBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        /// <summary>
        /// Closes work items as duplicates and links them to their canonical (original) work items using batch API.
        /// This method performs two operations per duplicate pair: closes the duplicate work item with appropriate
        /// state and reason, then creates a "Duplicate" relationship link to the canonical work item. This is
        /// commonly used in bug triage to maintain traceability while reducing duplicate work items.
        /// </summary>
        /// <param name="pairs">A collection of tuples containing duplicate work item IDs and their corresponding canonical work item IDs.</param>
        /// <param name="suppressNotifications">Whether to suppress email notifications for these operations (default: true).</param>
        /// <param name="bypassRules">Whether to bypass work item rules during closure and linking (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each operation,
        /// or error details if the batch operation fails. Each pair generates multiple operations that may succeed or fail independently.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when pairs collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to close or link work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or operations are invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseAndLinkDuplicatesBatchAsync(
            IEnumerable<(int duplicateId, int canonicalId)> pairs,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(pairs);

                    var batch = pairs.Select(pair =>
                        CreateCloseDuplicateRequest(pair.duplicateId, pair.canonicalId))
                        .ToList();

                    AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> inner = await ExecuteBatchAsync(batch, cancellationToken);
                    return inner.IsSuccessful ? inner.Value : [];
                }, CloseAndLinkDuplicatesBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        /// <summary>
        /// Retrieves multiple work items by their IDs using Azure DevOps batch API for optimal performance.
        /// This method fetches work items in a single batch request rather than individual API calls,
        /// significantly improving performance when retrieving multiple work items. You can specify
        /// which fields to retrieve and the level of expansion for related data.
        /// </summary>
        /// <param name="ids">A collection of work item IDs to retrieve.</param>
        /// <param name="expand">Specifies how much to expand in the response (default: All).</param>
        /// <param name="fields">Optional collection of specific field names to retrieve (null returns all fields).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of work items,
        /// or error details if the batch retrieval fails. Work items that don't exist will be omitted from results.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when ids collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to read work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when batch request format is invalid or exceeds limits.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> GetWorkItemsBatchByIdsAsync(
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WorkItem> workItems = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(ids);
                    var request = new WorkItemBatchGetRequest
                    {
                        Ids = ids.ToList(),
                        Expand = expand,
                        Fields = fields?.ToList()
                    };

                    List<WorkItem> result = await _workItemClient.GetWorkItemsBatchAsync(request, cancellationToken: cancellationToken);
                    return (IReadOnlyList<WorkItem>)result;
                }, GetWorkItemsBatchByIdsOperation, OperationType.Read);

                return CreateSuccessResult(workItems);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WorkItem>>(ex);
            }
        }

        /// <summary>
        /// Links multiple work items using batch API with human-readable relationship names.
        /// This method creates relationships between work items using friendly names (like "Related", "Parent", "Child")
        /// rather than system relationship types. Each link can include an optional comment explaining the relationship.
        /// The batch operation provides optimal performance when creating many links simultaneously.
        /// </summary>
        /// <param name="links">A collection of link definitions containing source ID, target ID, relationship type name, and optional comments.</param>
        /// <param name="suppressNotifications">Whether to suppress email notifications for these link operations (default: true).</param>
        /// <param name="bypassRules">Whether to bypass work item rules during link creation (default: false).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of batch responses for each link operation,
        /// or error details if the batch operation fails. Individual link operations may succeed or fail independently.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when links collection is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any link type name is invalid or cannot be mapped to a system relationship.</exception>
        /// <exception cref="HttpRequestException">Thrown when the batch API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to link work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or relationship types are invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsByNameBatchAsync(
            IEnumerable<(int sourceId, int targetId, string type, string? comment)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WitBatchResponse> responses = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    ArgumentNullException.ThrowIfNull(links);

                    var batch = links.Select(link =>
                    {
                        string relation = GetRelationFromName(link.type);
                        return CreateLinkRequest(link.sourceId, link.targetId, relation, link.comment ?? string.Empty, suppressNotifications, bypassRules);
                    }).ToList();

                    AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>> inner = await ExecuteBatchAsync(batch, cancellationToken);
                    return inner.IsSuccessful ? inner.Value : [];
                }, LinkWorkItemsByNameBatchOperation, OperationType.Update);

                return CreateSuccessResult(responses);
            }
            catch(Exception ex)
            {
                return CreateFailureResult<IReadOnlyList<WitBatchResponse>>(ex);
            }
        }

        // Helper methods to reduce duplication while preserving try-catch blocks

        private AzureDevOpsActionResult<T> CreateSuccessResult<T>(T value)
        {
            return AzureDevOpsActionResult<T>.Success(value, Logger);
        }

        private AzureDevOpsActionResult<T> CreateFailureResult<T>(Exception ex)
        {
            return AzureDevOpsActionResult<T>.Failure(ex, Logger);
        }

        private WitBatchRequest CreateWorkItemUpdateRequest(int workItemId, WorkItemCreateOptions options, bool suppressNotifications, bool bypassRules)
        {
            JsonPatchDocument patch = BuildPatchDocument(options);

            return new WitBatchRequest
            {
                Method = Constants.PatchMethod,
                Uri = BuildWorkItemApiUrl(workItemId, suppressNotifications, bypassRules),
                Headers = CreateJsonPatchHeaders(),
                Body = JsonSerializer.Serialize(patch)
            };
        }

        private WitBatchRequest CreateLinkRequest(int sourceId, int targetId, string relation, string comment, bool suppressNotifications, bool bypassRules)
        {
            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = RelationsPath,
                    Value = new
                    {
                        rel = relation,
                        url = BuildWorkItemTrackingUrl(targetId),
                        attributes = new { comment }
                    }
                }
            };

            return new WitBatchRequest
            {
                Method = Constants.PatchMethod,
                Uri = BuildWorkItemApiUrlWithParams(sourceId, suppressNotifications, bypassRules),
                Headers = CreateJsonPatchHeaders(),
                Body = JsonSerializer.Serialize(patch)
            };
        }

        private WitBatchRequest CreateCloseWorkItemRequest(int workItemId, string closedState, string? closedReason)
        {
            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = SystemStateFieldPath,
                    Value = closedState
                }
            };

            if(!string.IsNullOrWhiteSpace(closedReason))
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = SystemReasonFieldPath,
                    Value = closedReason
                });
            }

            return new WitBatchRequest
            {
                Method = Constants.PatchMethod,
                Uri = BuildWorkItemApiUrl(workItemId),
                Headers = CreateJsonPatchHeaders(),
                Body = JsonSerializer.Serialize(patch)
            };
        }

        private WitBatchRequest CreateCloseDuplicateRequest(int duplicateId, int canonicalId)
        {
            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = SystemStateFieldPath,
                    Value = DefaultClosedState
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = SystemReasonFieldPath,
                    Value = DefaultClosedReason
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = RelationsPath,
                    Value = new
                    {
                        rel = DuplicateLinkType,
                        url = BuildWorkItemTrackingUrl(canonicalId),
                        attributes = new { comment = DuplicateLinkComment }
                    }
                }
            };

            return new WitBatchRequest
            {
                Method = Constants.PatchMethod,
                Uri = BuildWorkItemApiUrl(duplicateId),
                Headers = CreateJsonPatchHeaders(),
                Body = JsonSerializer.Serialize(patch)
            };
        }

        private static void ValidateRelation(string relation)
        {
            if(string.IsNullOrWhiteSpace(relation))
            {
                throw new ArgumentException("links array parameter has a Relation that cannot be null or whitespace.", relation);
            }
        }

        private static Dictionary<string, string> CreateJsonPatchHeaders()
        {
            return new Dictionary<string, string>
            {
                { Constants.ContentTypeHeader, Constants.JsonPatchContentType }
            };
        }

        private static string BuildWorkItemApiUrl(int workItemId, bool suppressNotifications = false, bool bypassRules = false)
        {
            string baseUrl = $"/_apis/wit/workitems/{workItemId}?api-version={GlobalConstants.ApiVersion}";
            
            if(!suppressNotifications || !bypassRules)
            {
                return baseUrl;
            }

            return $"{baseUrl}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}";
        }

        private static string BuildWorkItemApiUrlWithParams(int workItemId, bool suppressNotifications, bool bypassRules)
        {
            return $"/_apis/wit/workitems/{workItemId}?api-version={GlobalConstants.ApiVersion}&bypassRules={bypassRules.ToString().ToLowerInvariant()}&suppressNotifications={suppressNotifications.ToString().ToLowerInvariant()}";
        }

        private static string BuildWorkItemTrackingUrl(int workItemId)
        {
            return $"vstfs:///WorkItemTracking/WorkItem/{workItemId}";
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

