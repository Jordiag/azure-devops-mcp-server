using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public partial class OverviewClient : IDisposable, IAsyncDisposable
    {
        private readonly string _projectName;
        private readonly ILogger _logger;
        private readonly DashboardHttpClient _dashboardHttpClient;
        private readonly ProjectHttpClient _projectHttpClient;
        private readonly WikiHttpClient _wikiHttpClient;
        private readonly VssConnection _connection;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the OverviewClient with comprehensive Azure DevOps integration for project overview, dashboard management, and wiki operations.
        /// Establishes authenticated connections to Azure DevOps services including dashboard management, project administration, and wiki content systems.
        /// Provides unified access point for project-wide overview functionality including summary information, visualization dashboards, and collaborative documentation.
        /// Essential for creating integrated Azure DevOps overview solutions with comprehensive project insights, team collaboration, and knowledge management capabilities.
        /// </summary>
        /// <param name="organizationUrl">Complete URL of the Azure DevOps organization including protocol and domain for service connection establishment and API access.</param>
        /// <param name="projectName">Name of the specific Azure DevOps project for scoped operations and context-aware service access across all overview functionalities.</param>
        /// <param name="personalAccessToken">Personal Access Token with appropriate permissions for authenticated access to project overview, dashboard, and wiki services within the organization.</param>
        /// <param name="logger">Optional logger instance for comprehensive operation tracking, error reporting, and diagnostic information across all overview client operations. Uses NullLogger if not provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when organizationUrl, projectName, or personalAccessToken are null or empty</exception>
        /// <exception cref="ArgumentException">Thrown when organizationUrl is malformed, projectName contains invalid characters, or personalAccessToken format is incorrect</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the personal access token lacks required permissions for overview operations or organization access</exception>
        /// <exception cref="VssServiceException">Thrown when connection to Azure DevOps services fails or organization/project cannot be validated</exception>
        public OverviewClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _logger = logger ?? NullLogger.Instance;
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(organizationUrl), credentials);
            _dashboardHttpClient = _connection.GetClient<DashboardHttpClient>();
            _projectHttpClient = _connection.GetClient<ProjectHttpClient>();
            _wikiHttpClient = _connection.GetClient<WikiHttpClient>();
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
