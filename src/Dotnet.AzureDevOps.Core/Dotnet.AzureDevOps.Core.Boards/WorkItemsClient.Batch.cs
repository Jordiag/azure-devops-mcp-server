using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<AzureDevOpsActionResult<IReadOnlyList<int>>> CreateWorkItemsMultipleCallsAsync(string workItemType, IEnumerable<WorkItemCreateOptions> items, CancellationToken cancellationToken = default)
        {
            try
            {
                var createdIds = new List<int>();
                foreach(WorkItemCreateOptions itemOptions in items)
                {
                    AzureDevOpsActionResult<int> id = await CreateWorkItemAsync(workItemType, itemOptions, cancellationToken: cancellationToken);
                    if(id.IsSuccessful)
                    {
                        createdIds.Add(id.Value);
                    }
                }

                return AzureDevOpsActionResult<IReadOnlyList<int>>.Success(createdIds, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<int>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> BulkUpdateWorkItemsAsync(IEnumerable<(int id, WorkItemCreateOptions options)> updates, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach((int id, WorkItemCreateOptions options) in updates)
                {
                    await UpdateWorkItemAsync(id, options, cancellationToken);
                }

                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> ExecuteBatchAsync(IEnumerable<WitBatchRequest> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(requests);
                List<WitBatchRequest> requestList = requests.ToList();

                List<WitBatchResponse> result = await _workItemClient
                    .ExecuteBatchRequest(requestList, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> UpdateWorkItemsBatchAsync(
            IEnumerable<(int id, WorkItemCreateOptions options)> updates,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(updates);

                List<WitBatchRequest> batch = new List<WitBatchRequest>();

                foreach((int id, WorkItemCreateOptions options) in updates)
                {
                    JsonPatchDocument patch = BuildPatchDocument(options);

                    WitBatchRequest request = new WitBatchRequest
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

                return await ExecuteBatchAsync(batch, cancellationToken);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsBatchAsync(
            IEnumerable<(int sourceId, int targetId, string relation)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if(links == null)
                {
                    throw new ArgumentNullException(nameof(links));
                }

                var batch = new List<WitBatchRequest>();

                foreach((int sourceId, int targetId, string relation) in links)
                {
                    if(string.IsNullOrWhiteSpace(relation))
                    {
                        throw new ArgumentException("Relation cannot be null or whitespace.", nameof(links));
                    }

                    JsonPatchDocument patch = new JsonPatchDocument
                    {
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/relations/-",
                            Value = new
                            {
                                rel = relation,
                                url = $"vstfs:///WorkItemTracking/WorkItem/{targetId}",
                                attributes = new { comment = "Linked by batch helper" }
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
                        Body = JsonSerializer.Serialize(patch)
                    };

                    batch.Add(request);
                }

                return await ExecuteBatchAsync(batch, cancellationToken);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseWorkItemsBatchAsync(
            IEnumerable<int> workItemIds,
            string closedState = "Closed",
            string? closedReason = "Duplicate",
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(workItemIds);

                List<WitBatchRequest> batch = new List<WitBatchRequest>();

                foreach(int id in workItemIds)
                {
                    JsonPatchDocument patch = new JsonPatchDocument
                    {
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.State",
                            Value = closedState
                        }
                    };

                    if(!string.IsNullOrWhiteSpace(closedReason))
                    {
                        patch.Add(new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.Reason",
                            Value = closedReason
                        });
                    }

                    WitBatchRequest request = new WitBatchRequest
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

                return await ExecuteBatchAsync(batch, cancellationToken);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> CloseAndLinkDuplicatesBatchAsync(
            IEnumerable<(int duplicateId, int canonicalId)> pairs,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(pairs);

                List<WitBatchRequest> batch = new List<WitBatchRequest>();

                foreach((int duplicateId, int canonicalId) in pairs)
                {
                    JsonPatchDocument patch = new JsonPatchDocument
                    {
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.State",
                            Value = "Closed"
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.Reason",
                            Value = "Duplicate"
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/relations/-",
                            Value = new
                            {
                                rel = "System.LinkTypes.Duplicate-Forward",
                                url = $"vstfs:///WorkItemTracking/WorkItem/{canonicalId}",
                                attributes = new { comment = "Marked duplicate via batch helper" }
                            }
                        }
                    };

                    WitBatchRequest request = new WitBatchRequest
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

                return await ExecuteBatchAsync(batch, cancellationToken);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItem>>> GetWorkItemsBatchByIdsAsync(
            IEnumerable<int> ids,
            WorkItemExpand expand = WorkItemExpand.All,
            IEnumerable<string>? fields = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(ids);

                WorkItemBatchGetRequest request = new WorkItemBatchGetRequest
                {
                    Ids = ids.ToList(),
                    Expand = expand,
                    Fields = fields?.ToList()
                };

                List<WorkItem> workItems = await _workItemClient.GetWorkItemsBatchAsync(request, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Success(workItems, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItem>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>> LinkWorkItemsByNameBatchAsync(
            IEnumerable<(int sourceId, int targetId, string type, string? comment)> links,
            bool suppressNotifications = true,
            bool bypassRules = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if(links == null)
                {
                    throw new ArgumentNullException(nameof(links));
                }

                List<WitBatchRequest> batch = new List<WitBatchRequest>();

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

                return await ExecuteBatchAsync(batch, cancellationToken);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WitBatchResponse>>.Failure(ex, _logger);
            }
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

