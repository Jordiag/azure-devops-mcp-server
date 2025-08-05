using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<AzureDevOpsActionResult<Guid>> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                using FileStream fileStream = File.OpenRead(filePath);
                AttachmentReference reference = await _workItemClient.CreateAttachmentAsync(fileStream, fileName: Path.GetFileName(filePath), cancellationToken: cancellationToken);

                JsonPatchDocument patch = new JsonPatchDocument
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
                return AzureDevOpsActionResult<Guid>.Success(reference.Id);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<Stream>> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                Stream content = await _workItemClient.GetAttachmentContentAsync(projectName, attachmentId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Stream>.Success(content);
            }
            catch(VssServiceException ex)
            {
                return AzureDevOpsActionResult<Stream>.Failure(ex);
            }
        }
    }
}

