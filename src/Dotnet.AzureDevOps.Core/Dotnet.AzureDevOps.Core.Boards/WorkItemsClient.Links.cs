using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Dotnet.AzureDevOps.Core.Common;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<AzureDevOpsActionResult<bool>> AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default)
        {
            try
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
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<WorkItem> itemResult = await GetWorkItemAsync(workItemId, cancellationToken);
            if(!itemResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<bool>.Failure(itemResult.ErrorMessage!, _logger);
            }

            WorkItem item = itemResult.Value;
            if(item.Relations == null)
            {
                return AzureDevOpsActionResult<bool>.Failure("Work item has no relations to remove.", _logger);
            }

            WorkItemRelation? relation = item.Relations.FirstOrDefault(r => r.Url == linkUrl);
            if(relation == null)
            {
                return AzureDevOpsActionResult<bool>.Failure("Link not found in work item relations.", _logger);
            }

            int index = item.Relations.IndexOf(relation);
            if(index < 0)
            {
                return AzureDevOpsActionResult<bool>.Failure("Invalid relation index.", _logger);
            }

            try
            {
                var patch = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Remove,
                        Path = $"/relations/{index}"
                    }
                };

                _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<WorkItem> itemResult = await GetWorkItemAsync(workItemId, cancellationToken);
            if(!itemResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>>.Failure(itemResult.ErrorMessage!, _logger);
            }

            WorkItem item = itemResult.Value;
            IReadOnlyList<WorkItemRelation> relations = (IReadOnlyList<WorkItemRelation>)(item.Relations ?? []);
            return AzureDevOpsActionResult<IReadOnlyList<WorkItemRelation>>.Success(relations, _logger);
        }

        public async Task<AzureDevOpsActionResult<bool>> LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default)
        {
            try
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
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }
    }
}