using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient : IReposClient
    {
        private readonly string _projectName;
        private readonly GitHttpClient _gitHttpClient;
        private readonly string _organizationUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;

        public ReposClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _organizationUrl = organizationUrl;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _gitHttpClient = connection.GetClient<GitHttpClient>();
            _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };
            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);
            _logger = logger ?? NullLogger.Instance;
        }
    }
}