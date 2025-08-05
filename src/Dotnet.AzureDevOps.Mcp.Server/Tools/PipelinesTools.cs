using System.ComponentModel;
using System.Collections.Generic;
using Dotnet.AzureDevOps.Core.Common;
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
    private static PipelinesClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new PipelinesClient(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Queues a new build run.")]
    public static Task<AzureDevOpsActionResult<int>> QueueRunAsync(string organizationUrl, string projectName, string personalAccessToken, BuildQueueOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.QueueRunAsync(options);
    }

    [McpServerTool, Description("Gets a build run by id.")]
    public static Task<AzureDevOpsActionResult<Build>> GetRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetRunAsync(buildId);
    }

    [McpServerTool, Description("Lists build runs.")]
    public static Task<AzureDevOpsActionResult<IReadOnlyList<Build>>> ListRunsAsync(string organizationUrl, string projectName, string personalAccessToken, BuildListOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListRunsAsync(options);
    }

    [McpServerTool, Description("Cancels a running build.")]
    public static Task<AzureDevOpsActionResult<bool>> CancelRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, TeamProjectReference project)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CancelRunAsync(buildId, project);
    }

    [McpServerTool, Description("Retries a completed build run.")]
    public static Task<AzureDevOpsActionResult<int>> RetryRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.RetryRunAsync(buildId);
    }

    [McpServerTool, Description("Downloads the console log for a build.")]
    public static Task<AzureDevOpsActionResult<string>> DownloadConsoleLogAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DownloadConsoleLogAsync(buildId);
    }

    [McpServerTool, Description("Creates a new pipeline definition.")]
    public static Task<AzureDevOpsActionResult<int>> CreatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, PipelineCreateOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreatePipelineAsync(options);
    }

    [McpServerTool, Description("Retrieves a pipeline definition.")]
    public static Task<AzureDevOpsActionResult<BuildDefinition>> GetPipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPipelineAsync(definitionId);
    }

    [McpServerTool, Description("Lists pipeline definitions.")]
    public static Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListPipelinesAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPipelinesAsync();
    }

    [McpServerTool, Description("Updates a pipeline definition.")]
    public static Task<AzureDevOpsActionResult<bool>> UpdatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, PipelineUpdateOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdatePipelineAsync(definitionId, options);
    }

    [McpServerTool, Description("Deletes a pipeline definition.")]
    public static Task<AzureDevOpsActionResult<bool>> DeletePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeletePipelineAsync(definitionId);
    }

    [McpServerTool, Description("Lists build definitions with advanced filters.")]
    public static Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListDefinitionsAsync(string organizationUrl, string projectName, string personalAccessToken, BuildDefinitionListOptions options)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListDefinitionsAsync(options);
    }

    [McpServerTool, Description("Gets definition revision history.")]
    public static Task<AzureDevOpsActionResult<List<BuildDefinitionRevision>>> GetDefinitionRevisionsAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetDefinitionRevisionsAsync(definitionId);
    }

    [McpServerTool, Description("Retrieves build logs.")]
    public static Task<AzureDevOpsActionResult<List<BuildLog>>> GetLogsAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetLogsAsync(buildId);
    }

    [McpServerTool, Description("Retrieves lines from a build log.")]
    public static Task<AzureDevOpsActionResult<List<string>>> GetLogLinesAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, int logId, int? startLine = null, int? endLine = null)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetLogLinesAsync(buildId, logId, startLine, endLine);
    }

    [McpServerTool, Description("Gets changes associated with a build.")]
    public static Task<AzureDevOpsActionResult<List<Change>>> GetChangesAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetChangesAsync(buildId, continuationToken, top, includeSourceChange);
    }

    [McpServerTool, Description("Retrieves the build report metadata.")]
    public static Task<AzureDevOpsActionResult<BuildReportMetadata>> GetBuildReportAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetBuildReportAsync(buildId);
    }

    [McpServerTool, Description("Updates the state of a build stage.")]
    public static Task<AzureDevOpsActionResult<bool>> UpdateBuildStageAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false)
    {
        PipelinesClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdateBuildStageAsync(buildId, stageName, status, forceRetryAllJobs);
    }
}

