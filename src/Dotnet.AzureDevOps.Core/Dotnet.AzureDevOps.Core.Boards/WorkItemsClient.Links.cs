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
        public async Task AddLinkAsync(int workItemId, int targetWorkItemId, string linkType, CancellationToken cancellationToken = default)
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
        }

        public async Task RemoveLinkAsync(int workItemId, string linkUrl, CancellationToken cancellationToken = default)
        {
            WorkItem? item = await GetWorkItemAsync(workItemId, cancellationToken);
            if(item?.Relations == null)
                return;

            WorkItemRelation? relation = item.Relations.FirstOrDefault(r => r.Url == linkUrl);
            if(relation == null)
                return;

            int index = item.Relations.IndexOf(relation);
            if(index < 0)
                return;

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Remove,
                    Path = $"/relations/{index}"
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<WorkItemRelation>> GetLinksAsync(int workItemId, CancellationToken cancellationToken = default)
        {
            WorkItem? item = await GetWorkItemAsync(workItemId, cancellationToken);
            return (IReadOnlyList<WorkItemRelation>)(item?.Relations ?? []);
        }

        public async Task LinkWorkItemToPullRequestAsync(string projectId, string repositoryId, int pullRequestId, int workItemId, CancellationToken cancellationToken = default)
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
        }
    }
}

