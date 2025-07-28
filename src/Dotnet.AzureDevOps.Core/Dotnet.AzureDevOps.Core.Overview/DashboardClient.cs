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

        public async Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(CancellationToken cancellationToken = default)
        {
            TeamContext teamContext = new TeamContext(_projectName);
            List<Dashboard> group = await _dashboardHttpClient.GetDashboardsByProjectAsync(teamContext, cancellationToken: cancellationToken);
            IReadOnlyList<Dashboard> dashboards = group?.Where(d => d != null).ToList() ?? [];
            return dashboards;
        }

        public async Task<Dashboard?> GetDashboardAsync(Guid dashboardId, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamContext teamContext = new TeamContext(_projectName);
                return await _dashboardHttpClient.GetDashboardAsync(teamContext, dashboardId, cancellationToken: cancellationToken);
            }
            catch(VssServiceException)
            {
                return null;
            }
        }
    }
}
