using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<AzureDevOpsActionResult<bool>> AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default)
        {
            try
            {
                CommentCreate commentCreate = new CommentCreate { Text = comment };
                _ = await _workItemClient.AddCommentAsync(commentCreate, projectName, workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemComment>>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemComments commentsResult = await _workItemClient.GetCommentsAsync(workItemId, cancellationToken: cancellationToken);
                IReadOnlyList<WorkItemComment> comments = commentsResult.Comments ?? Array.Empty<WorkItemComment>();
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemComment>>.Success(comments);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemComment>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<WorkItemUpdate> updates = await _workItemClient.GetUpdatesAsync(workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Success(updates);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Failure(ex);
            }
        }
    }
}

