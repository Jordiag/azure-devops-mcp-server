using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes project and process management operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ProjectSettingsTools
{
    private readonly IProjectSettingsClient _projectSettingsClient;
    private readonly ILogger<ProjectSettingsTools> _logger;

    public ProjectSettingsTools(IProjectSettingsClient projectSettingsClient, ILogger<ProjectSettingsTools> logger)
    {
        _projectSettingsClient = projectSettingsClient;
        _logger = logger;
    }

    [McpServerTool, Description("Creates a new team in the project if it does not exist already.")]
    public async Task<bool> CreateTeamIfDoesNotExistAsync(string teamName, string teamDescription)
    {
        return (await _projectSettingsClient.CreateTeamIfDoesNotExistAsync(teamName, teamDescription)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a team's identifier by name.")]
    public async Task<Guid> GetTeamIdAsync(string teamName)
    {
        return (await _projectSettingsClient.GetTeamIdAsync(teamName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets all teams in the project.")]
    public async Task<List<WebApiTeam>> GetAllTeamsAsync()
    {
        return (await _projectSettingsClient.GetAllTeamsAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Updates a team's description.")]
    public async Task<bool> UpdateTeamDescriptionAsync(string teamName, string newDescription)
    {
        return (await _projectSettingsClient.UpdateTeamDescriptionAsync(teamName, newDescription)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a team by identifier.")]
    public async Task<bool> DeleteTeamAsync(Guid teamGuid)
    {
        return (await _projectSettingsClient.DeleteTeamAsync(teamGuid)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates an inherited process from a system process.")]
    public async Task<bool> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName)
    {
        return (await _projectSettingsClient.CreateInheritedProcessAsync(newProcessName, description, baseProcessName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes an inherited process.")]
    public async Task<bool> DeleteInheritedProcessAsync(string processId)
    {
        return (await _projectSettingsClient.DeleteInheritedProcessAsync(processId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a process identifier by name.")]
    public async Task<string> GetProcessIdAsync(string processName)
    {
        return (await _projectSettingsClient.GetProcessIdAsync(processName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a new project.")]
    public async Task<Guid> CreateProjectAsync(string projectName, string description, string processId)
    {
        return (await _projectSettingsClient.CreateProjectAsync(projectName, description, processId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets project information.")]
    public async Task<TeamProject> GetProjectAsync(string projectName)
    {
        return (await _projectSettingsClient.GetProjectAsync(projectName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a project.")]
    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        return (await _projectSettingsClient.DeleteProjectAsync(projectId)).EnsureSuccess(_logger);
    }
}
