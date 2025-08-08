using System.ComponentModel;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes project and process management operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ProjectSettingsTools(IProjectSettingsClient projectSettingsClient, ILogger<ProjectSettingsTools> logger)
{
    private readonly IProjectSettingsClient _projectSettingsClient = projectSettingsClient;
    private readonly ILogger<ProjectSettingsTools> _logger = logger;

    [McpServerTool, Description("Creates a new team in the Azure DevOps project if it doesn't already exist. Teams organize users and define area paths, iterations, and dashboard access. Prevents duplicate creation by checking for existing team name. Returns true if team was created or already exists.")]
    public async Task<bool> CreateTeamIfDoesNotExistAsync(string teamName, string teamDescription) =>
        (await _projectSettingsClient.CreateTeamIfDoesNotExistAsync(teamName, teamDescription)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the unique identifier (GUID) of a team by its name. Team IDs are required for many Azure DevOps operations involving team-specific configurations, dashboards, or work item area paths.")]
    public async Task<Guid> GetTeamIdAsync(string teamName) =>
        (await _projectSettingsClient.GetTeamIdAsync(teamName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all teams in the Azure DevOps project with their details including names, descriptions, IDs, and member information. Useful for team management and understanding project organization structure.")]
    public async Task<List<WebApiTeam>> GetAllTeamsAsync() =>
        (await _projectSettingsClient.GetAllTeamsAsync()).EnsureSuccess(_logger);

    [McpServerTool, Description("Updates the description of an existing team. Team descriptions help users understand the purpose and responsibilities of each team within the project. Returns true if the update was successful.")]
    public async Task<bool> UpdateTeamDescriptionAsync(string teamName, string newDescription) =>
        (await _projectSettingsClient.UpdateTeamDescriptionAsync(teamName, newDescription)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a team from the Azure DevOps project. This removes team-specific configurations, dashboards, and area path assignments. Work items and other artifacts remain but lose team association. Returns true if deletion was successful.")]
    public async Task<bool> DeleteTeamAsync(Guid teamGuid) =>
        (await _projectSettingsClient.DeleteTeamAsync(teamGuid)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new inherited process template based on an existing system process (Agile, Scrum, CMMI, or Basic). Inherited processes allow customization of work item types, fields, states, and rules while maintaining compatibility with system processes. Returns true if the process was created successfully.")]
    public async Task<bool> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName) =>
        (await _projectSettingsClient.CreateInheritedProcessAsync(newProcessName, description, baseProcessName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes an inherited process template from Azure DevOps. Only inherited processes that are not being used by any projects can be deleted. System processes (Agile, Scrum, CMMI, Basic) cannot be deleted. Returns true if the process was successfully removed.")]
    public async Task<bool> DeleteInheritedProcessAsync(string processId) =>
        (await _projectSettingsClient.DeleteInheritedProcessAsync(processId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the unique identifier (GUID) of a process template by its name. Process IDs are required for creating projects and managing process customizations. Supports both system processes and inherited process templates.")]
    public async Task<string> GetProcessIdAsync(string processName) =>
        (await _projectSettingsClient.GetProcessIdAsync(processName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new Azure DevOps project with specified name, description, and process template. The project will be initialized with the chosen process (Agile, Scrum, CMMI, Basic, or inherited) which defines work item types and workflow. Returns the unique project GUID identifier.")]
    public async Task<Guid> CreateProjectAsync(string projectName, string description, string processId) =>
        (await _projectSettingsClient.CreateProjectAsync(projectName, description, processId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific Azure DevOps project including name, description, state, visibility, capabilities, and process template information. The project must exist and the caller must have read access to the project.")]
    public async Task<TeamProject> GetProjectAsync(string projectName) =>
        (await _projectSettingsClient.GetProjectAsync(projectName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes an Azure DevOps project and all its contents including repositories, work items, pipelines, and artifacts. This action cannot be undone. All project data will be lost. Returns true if deletion was successful.")]
    public async Task<bool> DeleteProjectAsync(Guid projectId) =>
        (await _projectSettingsClient.DeleteProjectAsync(projectId)).EnsureSuccess(_logger);
}
