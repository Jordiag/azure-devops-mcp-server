using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes pipeline operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class PipelinesTools
{
    private PipelinesClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Queues a new build run.")]
    public Task<int> QueueRunAsync(string organizationUrl, string projectName, string personalAccessToken, BuildQueueOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.QueueRunAsync(options);
    }

    [McpServerTool, Description("Gets a build run by id.")]
    public Task<Build?> GetRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetRunAsync(buildId);
    }

    [McpServerTool, Description("Lists build runs.")]
    public Task<IReadOnlyList<Build>> ListRunsAsync(string organizationUrl, string projectName, string personalAccessToken, BuildListOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListRunsAsync(options);
    }

    [McpServerTool, Description("Cancels a running build.")]
    public Task CancelRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, TeamProjectReference project)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CancelRunAsync(buildId, project);
    }

    [McpServerTool, Description("Retries a completed build run.")]
    public Task<int> RetryRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.RetryRunAsync(buildId);
    }

    [McpServerTool, Description("Downloads the console log for a build.")]
    public Task<string?> DownloadConsoleLogAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DownloadConsoleLogAsync(buildId);
    }

    [McpServerTool, Description("Creates a new pipeline definition.")]
    public Task<int> CreatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, PipelineCreateOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreatePipelineAsync(options);
    }

    [McpServerTool, Description("Retrieves a pipeline definition.")]
    public Task<BuildDefinition?> GetPipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPipelineAsync(definitionId);
    }

    [McpServerTool, Description("Lists pipeline definitions.")]
    public Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPipelinesAsync();
    }

    [McpServerTool, Description("Updates a pipeline definition.")]
    public Task UpdatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, PipelineUpdateOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdatePipelineAsync(definitionId, options);
    }

    [McpServerTool, Description("Deletes a pipeline definition.")]
    public Task DeletePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePipelineAsync(definitionId);
    }
}
