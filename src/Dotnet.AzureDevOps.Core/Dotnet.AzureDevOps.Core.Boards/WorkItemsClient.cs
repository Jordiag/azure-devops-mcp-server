using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient : IWorkItemsClient, IDisposable, IAsyncDisposable
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly ILogger _logger;
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private readonly WorkHttpClient _workClient;
        private readonly HttpClient _httpClient;
        private readonly VssConnection _connection;
        private bool _disposed;
        private const string _patchMethod = Constants.PatchMethod;
        private const string ContentTypeHeader = Constants.ContentTypeHeader;
        private const string JsonPatchContentType = Constants.JsonPatchContentType;

        public WorkItemsClient(HttpClient httpClient, string organizationUrl, string projectName, string personalAccessToken, ILogger<WorkItemsClient>? logger = null)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;
            _logger = (ILogger?)logger ?? NullLogger.Instance;

            _httpClient = httpClient;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _workItemClient = _connection.GetClient<WorkItemTrackingHttpClient>();
            _workClient = _connection.GetClient<WorkHttpClient>();
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
                string projectUrl = $"{_organizationUrl}/_apis/projects/{_projectName}?api-version={GlobalConstants.ApiVersion}&includeCapabilities=true";
                JsonElement projectResponse = await _httpClient.GetFromJsonAsync<JsonElement>(projectUrl, cancellationToken);

                string? processId = projectResponse
                    .GetProperty("capabilities")
                    .GetProperty("processTemplate")
                    .GetProperty("templateTypeId")
                    .GetString();

                if(string.IsNullOrEmpty(processId))
                {
                    return AzureDevOpsActionResult<bool>.Failure("Unable to determine the process ID for the project.");
                }

                string processUrl = $"{_organizationUrl}/_apis/process/processes/{processId}?api-version={GlobalConstants.ApiVersion}";
                JsonElement processResponse = await _httpClient.GetFromJsonAsync<JsonElement>(processUrl, cancellationToken);
                string? processType = processResponse.GetProperty("type").GetString();

                if(string.IsNullOrEmpty(processType))
                {
                    return AzureDevOpsActionResult<bool>.Failure("Unable to determine process type for the project.");
                }

                bool isSystem = processType.Equals("system", StringComparison.OrdinalIgnoreCase);
                return AzureDevOpsActionResult<bool>.Success(isSystem, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            _connection?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}

