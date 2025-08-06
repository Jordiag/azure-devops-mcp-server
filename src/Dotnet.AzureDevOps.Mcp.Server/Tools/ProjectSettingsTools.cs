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
public static class ProjectSettingsTools
{
    private static ProjectSettingsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    [McpServerTool, Description("Creates a new team in the project if it does not exist already.")]
    public static async Task<bool> CreateTeamIfDoesNotExistAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string teamDescription, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateTeamIfDoesNotExistAsync(teamName, teamDescription)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a team's identifier by name.")]
    public static async Task<Guid> GetTeamIdAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetTeamIdAsync(teamName)).EnsureSuccess();
    }

    [McpServerTool, Description("Updates a team's description.")]
    public static async Task<bool> UpdateTeamDescriptionAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string newDescription, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .UpdateTeamDescriptionAsync(teamName, newDescription)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a team by identifier.")]
    public static async Task<bool> DeleteTeamAsync(string organizationUrl, string projectName, string personalAccessToken, Guid teamGuid, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteTeamAsync(teamGuid)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates an inherited process from a system process.")]
    public static async Task<bool> CreateInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string newProcessName, string description, string baseProcessName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateInheritedProcessAsync(newProcessName, description, baseProcessName)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes an inherited process by identifier.")]
    public static async Task<bool> DeleteInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string processId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteInheritedProcessAsync(processId)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a process identifier by name.")]
    public static async Task<string> GetProcessIdAsync(string organizationUrl, string projectName, string personalAccessToken, string processName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetProcessIdAsync(processName)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a new project using a specified process.")]
    public static async Task<Guid> CreateProjectAsync(string organizationUrl, string projectName, string personalAccessToken, string newProjectName, string description, string processId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateProjectAsync(newProjectName, description, processId)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a project by identifier.")]
    public static async Task<bool> DeleteProjectAsync(string organizationUrl, string projectName, string personalAccessToken, Guid projectId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteProjectAsync(projectId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists all teams in the organization.")]
    public static async Task<IReadOnlyList<WebApiTeam>> GetAllTeamsAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetAllTeamsAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a project by name.")]
    public static async Task<TeamProject> GetProjectAsync(string organizationUrl, string projectName, string personalAccessToken, string targetProjectName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetProjectAsync(targetProjectName)).EnsureSuccess();
    }
}