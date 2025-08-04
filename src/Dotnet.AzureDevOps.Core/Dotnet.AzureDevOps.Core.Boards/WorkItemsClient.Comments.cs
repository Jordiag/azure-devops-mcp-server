using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task AddCommentAsync(int workItemId, string projectName, string comment, CancellationToken cancellationToken = default)
        {
            var commentCreate = new CommentCreate { Text = comment };
            _ = await _workItemClient.AddCommentAsync(commentCreate, projectName, workItemId, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            WorkItemComments commentsResult = await _workItemClient.GetCommentsAsync(workItemId, cancellationToken: cancellationToken);
            return (IReadOnlyList<WorkItemComment>)(commentsResult.Comments ?? []);
        }

        public async Task<IReadOnlyList<WorkItemUpdate>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            List<WorkItemUpdate> updates = await _workItemClient.GetUpdatesAsync(workItemId, cancellationToken: cancellationToken);
            return updates;
        }
    }
}

