using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class SummaryClient : ISummaryClient
    {
        private readonly string _projectName;
        private readonly ProjectHttpClient _projectHttpClient;

        public SummaryClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _projectName = projectName;
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _projectHttpClient = connection.GetClient<ProjectHttpClient>();
        }

        public async Task<AzureDevOpsActionResult<TeamProject>> GetProjectSummaryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                TeamProject project = await _projectHttpClient.GetProject(_projectName, includeCapabilities: true, includeHistory: false, userState: null);
                return AzureDevOpsActionResult<TeamProject>.Success(project);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex);
            }
        }
    }
}
