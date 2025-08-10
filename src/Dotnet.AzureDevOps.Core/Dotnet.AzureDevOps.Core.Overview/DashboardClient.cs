using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class DashboardClient : IDashboardClient, IDisposable, IAsyncDisposable
    {
        private readonly string _projectName;
        private readonly DashboardHttpClient _dashboardHttpClient;
        private readonly VssConnection _connection;
        private readonly ILogger _logger;
        private bool _disposed;

        public DashboardClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _logger = logger ?? NullLogger.Instance;
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(organizationUrl), credentials);
            _dashboardHttpClient = _connection.GetClient<DashboardHttpClient>();
        }

        /// <summary>
        /// Retrieves all dashboards available within the Azure DevOps project, providing comprehensive overview of project visualization and monitoring tools.
        /// Returns collection of dashboards including team dashboards, project dashboards, and custom visualization configurations for complete project insights.
        /// Essential for dashboard discovery, project overview management, and comprehensive visualization catalog access across all teams and organizational units.
        /// Enables programmatic access to project-wide dashboard inventory for reporting, automation, and dashboard management workflows within Azure DevOps projects.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the dashboard listing operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: Read-only list of Dashboard objects with complete metadata, configuration, and visualization details for all accessible project dashboards
        /// - Failure: Error details if dashboards cannot be retrieved due to permissions, service issues, or project access problems
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to list dashboards in the specified project</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps dashboard service encounters issues during dashboard enumeration or project validation</exception>
        /// <exception cref="TimeoutException">Thrown when the operation exceeds allowed time limits due to large dashboard datasets or service performance issues</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<Dashboard>>> ListDashboardsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var teamContext = new TeamContext(_projectName);
                List<Dashboard> group = await _dashboardHttpClient.GetDashboardsByProjectAsync(teamContext, cancellationToken: cancellationToken);
                IReadOnlyList<Dashboard> dashboards = group?.Where(d => d != null).ToList() ?? new List<Dashboard>();
                return AzureDevOpsActionResult<IReadOnlyList<Dashboard>>.Success(dashboards, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<Dashboard>>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves a specific dashboard from Azure DevOps by its unique identifier within a designated team context, providing detailed visualization configuration.
        /// Returns comprehensive dashboard information including widget configurations, layout specifications, permissions, and team-specific customizations.
        /// Essential for dashboard inspection, widget analysis, and team-specific visualization access within Azure DevOps dashboard management systems.
        /// Enables programmatic access to detailed dashboard specifications for reporting automation, dashboard migration, and configuration management workflows.
        /// </summary>
        /// <param name="dashboardId">Unique GUID identifier of the specific dashboard to retrieve from Azure DevOps dashboard management system.</param>
        /// <param name="teamName">Name of the team context under which the dashboard is organized and managed within the Azure DevOps project.</param>
        /// <param name="cancellationToken">Optional token to cancel the dashboard retrieval operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: Dashboard object with complete configuration, widget details, layout specifications, and team-specific customization information
        /// - Failure: Error details if dashboard cannot be found, access is denied, team context is invalid, or service issues occur during retrieval
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when dashboard ID is invalid, malformed, or team name is empty or invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access the dashboard within the specified team context</exception>
        /// <exception cref="VssServiceException">Thrown when the dashboard does not exist, team context is invalid, or Azure DevOps service encounters retrieval issues</exception>
        public async Task<AzureDevOpsActionResult<Dashboard>> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default)
        {
            try
            {
                var teamContext = new TeamContext(_projectName, teamName);
                Dashboard dashboard = await _dashboardHttpClient.GetDashboardAsync(teamContext, dashboardId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Dashboard>.Success(dashboard, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Dashboard>.Failure(ex, _logger);
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
