using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes project and process management operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ProjectSettingsTools
{
    private static ProjectSettingsClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a new team in the project.")]
    public static Task<bool> CreateTeamAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string teamDescription)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateTeamAsync(teamName, teamDescription);
    }

    [McpServerTool, Description("Gets a team's identifier by name.")]
    public static Task<Guid> GetTeamIdAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetTeamIdAsync(teamName);
    }

    [McpServerTool, Description("Updates a team's description.")]
    public static Task<bool> UpdateTeamDescriptionAsync(string organizationUrl, string projectName, string personalAccessToken, string teamName, string newDescription)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdateTeamDescriptionAsync(teamName, newDescription);
    }

    [McpServerTool, Description("Deletes a team by identifier.")]
    public static Task<bool> DeleteTeamAsync(string organizationUrl, string projectName, string personalAccessToken, Guid teamGuid)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteTeamAsync(teamGuid);
    }

    [McpServerTool, Description("Creates an inherited process from a system process.")]
    public static Task<bool> CreateInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string newProcessName, string description, string baseProcessName)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateInheritedProcessAsync(newProcessName, description, baseProcessName);
    }

    [McpServerTool, Description("Deletes an inherited process by identifier.")]
    public static Task<bool> DeleteInheritedProcessAsync(string organizationUrl, string projectName, string personalAccessToken, string processId)
    {
        ProjectSettingsClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteInheritedProcessAsync(processId);
    }
}
