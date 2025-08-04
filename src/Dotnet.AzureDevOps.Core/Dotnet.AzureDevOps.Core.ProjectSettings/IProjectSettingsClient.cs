using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettings
{
    public interface IProjectSettingsClient
    {
        Task<AzureDevOpsActionResult<bool>> CreateTeamAsync(string teamName, string teamDescription);
        Task<AzureDevOpsActionResult<Guid>> GetTeamIdAsync(string teamName);
        Task<AzureDevOpsActionResult<List<WebApiTeam>>> GetAllTeamsAsync();
        Task<AzureDevOpsActionResult<bool>> UpdateTeamDescriptionAsync(string teamName, string newDescription);
        Task<AzureDevOpsActionResult<bool>> DeleteTeamAsync(Guid teamGuid);
        Task<AzureDevOpsActionResult<bool>> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName);
        Task<AzureDevOpsActionResult<bool>> DeleteInheritedProcessAsync(string processId);
        Task<AzureDevOpsActionResult<string>> GetProcessIdAsync(string processName);
        Task<AzureDevOpsActionResult<Guid>> CreateProjectAsync(string projectName, string description, string processId);
        Task<AzureDevOpsActionResult<TeamProject>> GetProjectAsync(string projectName);
        Task<AzureDevOpsActionResult<bool>> DeleteProjectAsync(Guid projectId);
    }
}
