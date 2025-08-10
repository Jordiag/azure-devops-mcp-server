using Dotnet.AzureDevOps.Core.Common;
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
        /// <exception cref="ArgumentException">Thrown when workItemId is invalid or filePath is null/empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="HttpRequestException">Thrown when the upload or attachment API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to attach files to work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the work item doesn't exist or file upload limits are exceeded.</exception>
        public async Task<AzureDevOpsActionResult<Guid>> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default)
        {
            try
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
                return AzureDevOpsActionResult<Guid>.Success(reference.Id, Logger);
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
        /// <exception cref="ArgumentException">Thrown when projectName is null/empty or attachmentId is empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the download API request fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access attachments.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the attachment doesn't exist or has been corrupted.</exception>
        /// <exception cref="VssServiceException">Thrown for Azure DevOps service-specific errors during download.</exception>
        public async Task<AzureDevOpsActionResult<Stream>> GetAttachmentAsync(string projectName, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                Stream content = await _workItemClient.GetAttachmentContentAsync(projectName, attachmentId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Stream>.Success(content, Logger);
            }
            catch(VssServiceException ex)
            {
                return AzureDevOpsActionResult<Stream>.Failure(ex, Logger);
            }
        }
    }
}

