using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient : AzureDevOpsClientBase, IReposClient
    {
        private readonly GitHttpClient _gitHttpClient;
        private readonly HttpClient _httpClient;

        public ReposClient(HttpClient httpClient, string organizationUrl, string projectName, string personalAccessToken, ILogger<ReposClient>? logger = null)
            : base(organizationUrl, personalAccessToken, projectName, logger)
        {
            _gitHttpClient = Connection.GetClient<GitHttpClient>();
            _httpClient = httpClient;
        }
    }
}