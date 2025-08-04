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
        public async Task<IReadOnlyList<int>> CreateWorkItemsMultipleCallsAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default)
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

            List<WitBatchResponse> result = await _workItemClient
                .ExecuteBatchRequest(requestList, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return result;
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
                    Uri = $"/_apis/wit/workitems/{id}?api-version={GlobalConstants.ApiVersion}",
                    Headers = new Dictionary<string, string>
                    {
                        { ContentTypeHeader, JsonPatchContentType }
                    },
                    Body = JsonSerializer.Serialize(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Adds work-item links (parent-child, duplicate, related, etc.) for many items in one $batch request.
        /// </summary>
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
                    Body = JsonSerializer.Serialize(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Closes a list of work items by setting <c>System.State</c> (and optionally <c>System.Reason</c>) in a single $batch request.
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
                    Uri = $"/_apis/wit/workitems/{id}?api-version={GlobalConstants.ApiVersion}",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json-patch+json" }
                    },
                    Body = JsonSerializer.Serialize(patch)
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
                    Uri = $"/_apis/wit/workitems/{duplicateId}?api-version={GlobalConstants.ApiVersion}",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json-patch+json" }
                    },
                    Body = JsonSerializer.Serialize(patch)
                };

                batch.Add(request);
            }

            return ExecuteBatchAsync(batch, cancellationToken);
        }

        /// <summary>
        /// Fetches up to 200 work‑items by ID via the read‑side <c>/workitemsbatch</c> endpoint – a single POST request instead of N individual GETs.
        /// </summary>
        public async Task<IReadOnlyList<WorkItem>> GetWorkItemsBatchByIdsAsync(
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var request = new WorkItemBatchGetRequest
            {
                Ids = ids.ToList(),
                Expand = expand,
                Fields = fields?.ToList()
            };

            WorkItemBatchGetResponse response = await _workItemClient.GetWorkItemsBatchAsync(request, cancellationToken: cancellationToken);
            return response.Value ?? [];
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
                    Uri = $"/_apis/wit/workitems/{sourceId}?api-version={GlobalConstants.ApiVersion}",
                    Headers = new Dictionary<string, string>
                    {
                        { ContentTypeHeader, JsonPatchContentType }
                    },
                    Body = JsonSerializer.Serialize(patch)
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

