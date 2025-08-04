using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<Guid?> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            AttachmentReference reference = await _workItemClient.CreateAttachmentAsync(fileStream, fileName: Path.GetFileName(filePath), cancellationToken: cancellationToken);

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = Constants.JsonPatchOperationPath,
                    Value = new
                    {
                        rel = "AttachedFile",
                        url = reference.Url
                    }
                }
            };

            _ = await _workItemClient.UpdateWorkItemAsync(patch, workItemId, cancellationToken: cancellationToken);
            return reference.Id;
        }

        public async Task<Stream?> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _workItemClient.GetAttachmentContentAsync(projectName, attachmentId, cancellationToken: cancellationToken);
            }
            catch(VssServiceException)
            {
                return null;
            }
        }
    }
}

