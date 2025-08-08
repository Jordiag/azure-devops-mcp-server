using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class SummaryClient : ISummaryClient
    {
        private readonly string _projectName;
        private readonly ProjectHttpClient _projectHttpClient;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the SummaryClient with authenticated Azure DevOps connection for project summary and metadata operations.
        /// Establishes secure connection to Azure DevOps Project HTTP client enabling comprehensive project information retrieval and analysis.
        /// Provides dedicated access to project-level summary data including capabilities, configuration, and organizational metadata within Azure DevOps.
        /// Essential for creating project overview solutions, administrative reporting, and comprehensive project analysis workflows.
        /// </summary>
        /// <param name="organizationUrl">Complete URL of the Azure DevOps organization including protocol and domain for secure service connection and API access.</param>
        /// <param name="projectName">Name of the specific Azure DevOps project for scoped summary operations and project-specific data retrieval.</param>
        /// <param name="personalAccessToken">Personal Access Token with project read permissions for authenticated access to project summary and metadata services.</param>
        /// <param name="logger">Optional logger instance for operation tracking, error reporting, and diagnostic information during summary operations. Uses NullLogger if not provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when organizationUrl, projectName, or personalAccessToken are null or empty</exception>
        /// <exception cref="ArgumentException">Thrown when organizationUrl is malformed, projectName contains invalid characters, or personalAccessToken format is incorrect</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the personal access token lacks required permissions for project access or summary operations</exception>
        /// <exception cref="VssServiceException">Thrown when connection to Azure DevOps project services fails or organization validation encounters issues</exception>
        public SummaryClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _projectHttpClient = connection.GetClient<ProjectHttpClient>();
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Retrieves comprehensive project summary information from Azure DevOps including metadata, capabilities, configuration, and organizational details.
        /// Returns complete TeamProject object with project settings, enabled services, process templates, and administrative configuration for project overview analysis.
        /// Essential for project discovery, capability assessment, configuration validation, and comprehensive project information gathering within Azure DevOps organizations.
        /// Enables programmatic access to project-level details for reporting, automation, project analysis, and administrative management workflows.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the project summary retrieval operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: TeamProject object with complete project metadata, capabilities, configuration, process template details, and organizational structure information
        /// - Failure: Error details if project summary cannot be retrieved due to permissions, invalid project name, or service accessibility issues
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access project information or capabilities within the Azure DevOps organization</exception>
        /// <exception cref="VssServiceException">Thrown when the project does not exist, project name is invalid, or Azure DevOps service encounters issues during project data retrieval</exception>
        /// <exception cref="ArgumentException">Thrown when the project name is malformed, empty, or references a non-existent project within the organization</exception>
        public async Task<AzureDevOpsActionResult<TeamProject>> GetProjectSummaryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                TeamProject project = await _projectHttpClient.GetProject(_projectName, includeCapabilities: true, includeHistory: false, userState: null);
                return AzureDevOpsActionResult<TeamProject>.Success(project, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex, _logger);
            }
        }
    }
}