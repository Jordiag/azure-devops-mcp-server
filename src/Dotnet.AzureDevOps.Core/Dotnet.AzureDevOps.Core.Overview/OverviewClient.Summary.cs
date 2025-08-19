using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public partial class OverviewClient
    {
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
                TeamProject project = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await this._projectHttpClient.GetProject(this.ProjectName, includeCapabilities: true, includeHistory: false, userState: null);
                }, "GetProjectSummary", OperationType.Read);

                return AzureDevOpsActionResult<TeamProject>.Success(project, this.Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex, this.Logger);
            }
        }
    }
}

