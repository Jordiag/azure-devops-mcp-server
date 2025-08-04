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

