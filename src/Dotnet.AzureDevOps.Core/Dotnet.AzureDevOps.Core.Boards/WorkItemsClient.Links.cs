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
        /// <summary>
        /// Creates a relationship link between two work items using the specified link type.
        /// Work item links help organize work hierarchically and track dependencies between different
        /// work items. Common link types include parent-child relationships, related items, duplicates,
        /// and predecessor-successor dependencies. Links are bidirectional and appear on both work items.
        /// </summary>
        /// <param name="workItemId">The ID of the source work item that will contain the link.</param>
        /// <param name="targetWorkItemId">The ID of the target work item to link to.</param>
        /// <param name="linkType">The type of relationship to create (e.g., "System.LinkTypes.Related", "System.LinkTypes.Hierarchy-Forward").</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the link was successfully created,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId or targetWorkItemId is invalid, or linkType is null/empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to link work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when work items don't exist or the link type is invalid.</exception>
        public async Task<AzureDevOpsActionResult<bool>> AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default)
        {
            try
            {
                bool result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var patch = new JsonPatchDocument
                    {
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = Constants.JsonPatchOperationPath,
                            Value = new { rel = linkType, url = $"{OrganizationUrl}/{ProjectName}/_apis/wit/workItems/{targetWorkItemId}" }
                        }
                    };
                    _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
                    return true;
                }, "AddLink", OperationType.Update);

                return AzureDevOpsActionResult<bool>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Removes a specific relationship link from a work item by its URL reference.
        /// This method first retrieves the work item to find the exact link to remove, then updates
        /// the work item to remove that relationship. The link removal is unidirectional - it only
        /// removes the link from the source work item, not the corresponding reverse link on the target.
        /// </summary>
        /// <param name="workItemId">The ID of the work item from which to remove the link.</param>
        /// <param name="linkUrl">The URL of the link to remove (typically the target work item's URL).</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the link was successfully removed,
        /// or error details if the operation fails or the link doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid or linkUrl is null/empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify work item links.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist, has no relations, or the specified link is not found.</exception>
        public async Task<AzureDevOpsActionResult<bool>> RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<WorkItem> itemResult = await GetWorkItemAsync(workItemId, cancellationToken);
            if(!itemResult.IsSuccessful)
                return AzureDevOpsActionResult<bool>.Failure(itemResult.ErrorMessage!, Logger);

            WorkItem item = itemResult.Value;
            if(item.Relations == null)
                return AzureDevOpsActionResult<bool>.Failure("Work item has no relations to remove.", Logger);

            WorkItemRelation? relation = item.Relations.FirstOrDefault(r => r.Url == linkUrl);
            if(relation == null)
                return AzureDevOpsActionResult<bool>.Failure("Link not found in work item relations.", Logger);

            int index = item.Relations.IndexOf(relation);
            if(index < 0)
                return AzureDevOpsActionResult<bool>.Failure("Invalid relation index.", Logger);

            try
            {
                bool result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var patch = new JsonPatchDocument
                    {
                        new JsonPatchOperation { Operation = Operation.Remove, Path = $"/relations/{index}" }
                    };
                    _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
                    return true;
                }, "RemoveLink", OperationType.Update);

                return AzureDevOpsActionResult<bool>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Links a work item to a pull request, establishing a connection between the two entities.
        /// This is accomplished by creating an artifact link of type "Pull Request" in the work item
        /// that references the pull request's VSTS URL. The operation is asynchronous and can be
        /// cancelled using the provided cancellation token.
        /// </summary>
        /// <param name="projectId">The ID of the project containing the repository.</param>
        /// <param name="repositoryId">The ID of the repository containing the pull request.</param>
        /// <param name="pullRequestId">The ID of the pull request to link to.</param>
        /// <param name="workItemId">The ID of the work item to update with the link.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the link was successfully created,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectId, repositoryId, pullRequestId, or workItemId is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify the work item.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist or the pull request link already exists.</exception>
        public async Task<AzureDevOpsActionResult<bool>> LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool result = await ExecuteWithExceptionHandlingAsync(async () =>
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
                            Value = new { rel = "ArtifactLink", url = vstfsUrl, attributes = new { name = "Pull Request" } }
                        }
                    };

                    _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
                    return true;
                }, "LinkWorkItemToPullRequest", OperationType.Update);

                return AzureDevOpsActionResult<bool>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }
    }
}
