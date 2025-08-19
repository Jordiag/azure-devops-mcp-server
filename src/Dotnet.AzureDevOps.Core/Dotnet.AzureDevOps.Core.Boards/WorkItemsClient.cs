using System.Net.Http.Json;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient : AzureDevOpsClientBase, IWorkItemsClient
    {
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private readonly WorkHttpClient _workClient;
        private readonly HttpClient _httpClient;
        private const string _patchMethod = Constants.PatchMethod;
        private const string ContentTypeHeader = Constants.ContentTypeHeader;
        private const string JsonPatchContentType = Constants.JsonPatchContentType;

        public WorkItemsClient(HttpClient httpClient, string organizationUrl, string projectName, string personalAccessToken, ILogger<WorkItemsClient>? logger = null)
            : base(organizationUrl, personalAccessToken, projectName, logger)
        {
            _httpClient = httpClient;
            _workItemClient = Connection.GetClient<WorkItemTrackingHttpClient>();
            _workClient = Connection.GetClient<WorkHttpClient>();
        }

        /// <summary>
        /// Determines whether the Azure DevOps project uses a system-managed process template.
        /// System processes are predefined templates (Agile, Scrum, CMMI) that cannot be customized,
        /// while inherited processes allow for customization of work item types, fields, and workflow states.
        /// This information is crucial for understanding customization capabilities and compliance requirements.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the project uses a system process template,
        /// false if it uses an inherited/custom process, or error details if the operation fails.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the API requests fail.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access project or process information.</exception>
        /// <exception cref="JsonException">Thrown when the API response cannot be parsed correctly.</exception>
        public async Task<AzureDevOpsActionResult<bool>> IsSystemProcessAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                bool isSystem = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    string projectUrl = $"{OrganizationUrl}/_apis/projects/{ProjectName}?api-version={GlobalConstants.ApiVersion}&includeCapabilities=true";
                    JsonElement projectResponse = await _httpClient.GetFromJsonAsync<JsonElement>(projectUrl, cancellationToken);
                    string? processId = projectResponse.GetProperty("capabilities").GetProperty("processTemplate").GetProperty("templateTypeId").GetString();
                    if(string.IsNullOrEmpty(processId))
                        throw new InvalidOperationException("Unable to determine the process ID for the project.");

                    string processUrl = $"{OrganizationUrl}/_apis/process/processes/{processId}?api-version={GlobalConstants.ApiVersion}";
                    JsonElement processResponse = await _httpClient.GetFromJsonAsync<JsonElement>(processUrl, cancellationToken);
                    string? processType = processResponse.GetProperty("type").GetString();
                    if(string.IsNullOrEmpty(processType))
                        throw new InvalidOperationException("Unable to determine process type for the project.");

                    return processType.Equals("system", StringComparison.OrdinalIgnoreCase);
                }, "IsSystemProcess", OperationType.Read);

                return AzureDevOpsActionResult<bool>.Success(isSystem, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }
    }
}


