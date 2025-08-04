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
    public partial class WorkItemsClient : IWorkItemsClient
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private readonly WorkHttpClient _workClient;
        private readonly HttpClient _httpClient;
        private const string _patchMethod = Constants.PatchMethod;
        private const string ContentTypeHeader = Constants.ContentTypeHeader;
        private const string JsonPatchContentType = Constants.JsonPatchContentType;

        public WorkItemsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;

            _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };

            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
            _workClient = connection.GetClient<WorkHttpClient>();
        }

        public async Task<bool> IsSystemProcessAsync(CancellationToken cancellationToken = default)
        {
            // Include process template details
            string projectUrl = $"{_organizationUrl}/_apis/projects/{_projectName}?api-version={GlobalConstants.ApiVersion}&includeCapabilities=true";
            JsonElement projectResponse = await _httpClient.GetFromJsonAsync<JsonElement>(projectUrl, cancellationToken);

            string? processId = projectResponse
                .GetProperty("capabilities")
                .GetProperty("processTemplate")
                .GetProperty("templateTypeId")
                .GetString();

            if(string.IsNullOrEmpty(processId))
            {
                throw new InvalidOperationException("Unable to determine the process ID for the project.");
            }

            string processUrl = $"{_organizationUrl}/_apis/process/processes/{processId}?api-version={GlobalConstants.ApiVersion}";
            JsonElement processResponse = await _httpClient.GetFromJsonAsync<JsonElement>(processUrl, cancellationToken);
            string? processType = processResponse.GetProperty("type").GetString();

            return string.IsNullOrEmpty(processType)
                ? throw new InvalidOperationException("Unable to determine process type for the project.")
                : processType.Equals("system", StringComparison.OrdinalIgnoreCase);
        }
    }
}

