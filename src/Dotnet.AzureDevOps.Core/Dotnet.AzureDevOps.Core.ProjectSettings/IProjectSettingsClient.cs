namespace Dotnet.AzureDevOps.Core.ProjectSettings
{
    public interface IProjectSettingsClient
    {
        Task<bool> CreateTeamAsync(string teamName, string teamDescription);
        Task<Guid> GetTeamIdAsync(string teamName);
        Task<bool> UpdateTeamDescriptionAsync(string teamName, string newDescription);
        Task<bool> DeleteTeamAsync(Guid teamGuid);
        Task<bool> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName);
        Task<bool> DeleteInheritedProcessAsync(string processId);
        Task<string?> GetProcessIdAsync(string processName);
        Task<Guid?> CreateProjectAsync(string projectName, string description, string processId);
        Task<bool> DeleteProjectAsync(Guid projectId);
    }
}
