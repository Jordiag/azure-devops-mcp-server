using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        /// <summary>
        /// Adds a file attachment to a work item by uploading the file and creating an attachment reference.
        /// This method handles the two-step process of uploading the file to Azure DevOps storage and then
        /// linking it to the specified work item. The attachment becomes part of the work item's history
        /// and can be accessed by team members with appropriate permissions.
        /// </summary>
        /// <param name="workItemId">The ID of the work item to attach the file to.</param>
        /// <param name="filePath">The local file system path to the file to be attached.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique identifier (GUID) of the created attachment,
        /// or error details if the upload or attachment operation fails.
        /// </returns>
        public async Task<AzureDevOpsActionResult<Guid>> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid attachmentId = await ExecuteWithExceptionHandlingAsync(async () =>
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
                }, "AddAttachment", OperationType.Update);

                return AzureDevOpsActionResult<Guid>.Success(attachmentId, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the content stream for a work item attachment by its unique identifier.
        /// This method downloads the actual file content associated with an attachment, allowing
        /// you to access, save, or process the attached file. The returned stream should be properly
        /// disposed of after use to free system resources.
        /// </summary>
        /// <param name="projectName">The name of the project containing the attachment.</param>
        /// <param name="attachmentId">The unique identifier (GUID) of the attachment to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a stream with the attachment content,
        /// or error details if the download fails or the attachment doesn't exist.
        /// </returns>
        public async Task<AzureDevOpsActionResult<Stream>> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                Stream content = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _workItemClient.GetAttachmentContentAsync(projectName, attachmentId, cancellationToken: cancellationToken);
                }, "GetAttachment", OperationType.Read);

                return AzureDevOpsActionResult<Stream>.Success(content, Logger);
            }
            catch(VssServiceException ex)
            {
                return AzureDevOpsActionResult<Stream>.Failure(ex, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Stream>.Failure(ex, Logger);
            }
        }
    }
}

