using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        /// <summary>
        /// Adds a comment to a work item, creating a new entry in the work item's discussion thread.
        /// Comments provide a way for team members to communicate about work items, ask questions,
        /// provide updates, or document decisions. The comment becomes part of the work item's
        /// permanent record and is visible to all users with access to the work item.
        /// </summary>
        /// <param name="workItemId">The ID of the work item to add the comment to.</param>
        /// <param name="projectName">The name of the project containing the work item.</param>
        /// <param name="comment">The text content of the comment to add.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the comment was successfully added,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid, projectName is null/empty, or comment is null/empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to comment on work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist or commenting is not allowed.</exception>
        public async Task<AzureDevOpsActionResult<bool>> AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default)
        {
            try
            {
                var commentCreate = new CommentCreate { Text = comment };
                _ = await _workItemClient.AddCommentAsync(commentCreate, projectName, workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves all comments associated with a work item, providing the complete discussion history.
        /// Comments are returned in chronological order and include metadata such as author, creation date,
        /// and comment content. This is useful for understanding the conversation flow and decision-making
        /// process around a work item.
        /// </summary>
        /// <param name="workItemId">The ID of the work item whose comments to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a collection of work item comments,
        /// or an empty collection if no comments exist, or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to read work item comments.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IEnumerable<WorkItemComment>>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemComments commentsResult = await _workItemClient.GetCommentsAsync(workItemId, cancellationToken: cancellationToken);
                IEnumerable<WorkItemComment> comments = commentsResult.Comments ?? Array.Empty<WorkItemComment>();
                return AzureDevOpsActionResult<IEnumerable<WorkItemComment>>.Success(comments, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IEnumerable<WorkItemComment>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the complete history of changes made to a work item, including field updates, state transitions,
        /// and other modifications. Each update entry contains information about what changed, when it changed,
        /// and who made the change. This provides a comprehensive audit trail for work item modifications,
        /// which is essential for tracking progress and understanding the work item's evolution over time.
        /// </summary>
        /// <param name="workItemId">The ID of the work item whose history to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of work item updates in chronological order,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to read work item history.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<WorkItemUpdate> updates = await _workItemClient.GetUpdatesAsync(workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Success(updates, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Failure(ex, Logger);
            }
        }
    }
}
