using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class DashboardClient : IDashboardClient
    {
        private readonly string _projectName;
        private readonly DashboardHttpClient _dashboardHttpClient;

        public DashboardClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _projectName = projectName;
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _dashboardHttpClient = connection.GetClient<DashboardHttpClient>();
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<Dashboard>>> ListDashboardsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                TeamContext teamContext = new TeamContext(_projectName);
                List<Dashboard> group = await _dashboardHttpClient.GetDashboardsByProjectAsync(teamContext, cancellationToken: cancellationToken);
                IReadOnlyList<Dashboard> dashboards = group?.Where(d => d != null).ToList() ?? new List<Dashboard>();
                return AzureDevOpsActionResult<IReadOnlyList<Dashboard>>.Success(dashboards);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<Dashboard>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<Dashboard>> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamContext teamContext = new TeamContext(_projectName, teamName);
                Dashboard dashboard = await _dashboardHttpClient.GetDashboardAsync(teamContext, dashboardId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Dashboard>.Success(dashboard);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Dashboard>.Failure(ex);
            }
        }
    }
}
