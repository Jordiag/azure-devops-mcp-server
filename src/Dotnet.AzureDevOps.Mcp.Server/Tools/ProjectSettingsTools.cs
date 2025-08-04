using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes project and process management operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public static class ProjectSettingsTools
{
    private static ProjectSettingsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new team in the project.")]
    public static async Task CreateTeamAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string teamDescription)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.CreateTeamAsync(teamName, teamDescription);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create team.");
    }

    [McpServerTool, Description("Gets a team's identifier by name.")]
    public static async Task<Guid?> GetTeamIdAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Guid> result = await client.GetTeamIdAsync(teamName);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Updates a team's description.")]
    public static async Task UpdateTeamDescriptionAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string newDescription)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.UpdateTeamDescriptionAsync(teamName, newDescription);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to update team description.");
    }

    [McpServerTool, Description("Deletes a team by identifier.")]
    public static async Task DeleteTeamAsync(string organizationUrl, string projectName, string personalAccessToken, Guid teamGuid)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteTeamAsync(teamGuid);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete team.");
    }

    [McpServerTool, Description("Creates an inherited process from a system process.")]
    public static async Task CreateInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string newProcessName, string description, string baseProcessName)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.CreateInheritedProcessAsync(newProcessName, description, baseProcessName);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create inherited process.");
    }

    [McpServerTool, Description("Deletes an inherited process by identifier.")]
    public static async Task DeleteInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string processId)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteInheritedProcessAsync(processId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete inherited process.");
    }

    [McpServerTool, Description("Gets a process identifier by name.")]
    public static async Task<string?> GetProcessIdAsync(string organizationUrl, string projectName, string personalAccessToken, string processName)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<string> result = await client.GetProcessIdAsync(processName);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Creates a new project using a specified process.")]
    public static async Task<Guid> CreateProjectAsync(string organizationUrl, string projectName, string personalAccessToken, string newProjectName, string description, string processId)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<Guid> result = await client.CreateProjectAsync(newProjectName, description, processId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create project.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a project by identifier.")]
    public static async Task DeleteProjectAsync(string organizationUrl, string projectName, string personalAccessToken, Guid projectId)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteProjectAsync(projectId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete project.");
    }
}
