using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettings;

public interface IProjectSettingsClient
{
    Task<AzureDevOpsActionResult<bool>> CreateTeamIfDoesNotExistAsync(string teamName, string teamDescription, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Guid>> GetTeamIdAsync(string teamName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<List<WebApiTeam>>> GetAllTeamsAsync(CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> UpdateTeamDescriptionAsync(string teamName, string newDescription, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteTeamAsync(Guid teamGuid, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteInheritedProcessAsync(string processId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<string>> GetProcessIdAsync(string processName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Guid>> CreateProjectAsync(string projectName, string description, string processId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<TeamProject>> GetProjectAsync(string projectName, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}

