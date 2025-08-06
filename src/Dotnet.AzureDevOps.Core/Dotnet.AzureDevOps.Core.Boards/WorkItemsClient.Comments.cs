using System.Collections.Generic;
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
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IEnumerable<WorkItemComment>>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                WorkItemComments commentsResult = await _workItemClient.GetCommentsAsync(workItemId, cancellationToken: cancellationToken);
                IEnumerable<WorkItemComment> comments = commentsResult.Comments ?? Array.Empty<WorkItemComment>();
                return AzureDevOpsActionResult<IEnumerable<WorkItemComment>>.Success(comments, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IEnumerable<WorkItemComment>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<WorkItemUpdate> updates = await _workItemClient.GetUpdatesAsync(workItemId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Success(updates, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemUpdate>>.Failure(ex, _logger);
            }
        }
    }
}