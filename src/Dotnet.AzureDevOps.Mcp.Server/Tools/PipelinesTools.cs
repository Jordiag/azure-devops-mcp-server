using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.Extensions.Logging;
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
    private readonly IPipelinesClient _pipelinesClient;
    private readonly ILogger<PipelinesTools> _logger;

    public PipelinesTools(IPipelinesClient pipelinesClient, ILogger<PipelinesTools> logger)
    {
        _pipelinesClient = pipelinesClient;
        _logger = logger;
    }

    [McpServerTool, Description("Queues a new build run.")]
    public async Task<int> QueueRunAsync(BuildQueueOptions options)
    {
        return (await _pipelinesClient.QueueRunAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a build run by id.")]
    public async Task<Build> GetRunAsync(int buildId)
    {
        return (await _pipelinesClient.GetRunAsync(buildId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists build runs.")]
    public async Task<IReadOnlyList<Build>> ListRunsAsync(BuildListOptions options)
    {
        return (await _pipelinesClient.ListRunsAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Cancels a running build.")]
    public async Task<bool> CancelRunAsync(int buildId, TeamProjectReference project)
    {
        return (await _pipelinesClient.CancelRunAsync(buildId, project)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retries a completed build run.")]
    public async Task<int> RetryRunAsync(int buildId)
    {
        return (await _pipelinesClient.RetryRunAsync(buildId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets build definition by id.")]
    public async Task<BuildDefinition> GetPipelineAsync(int definitionId)
    {
        return (await _pipelinesClient.GetPipelineAsync(definitionId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists build definitions.")]
    public async Task<IReadOnlyList<BuildDefinitionReference>> ListDefinitionsAsync(BuildDefinitionListOptions options)
    {
        return (await _pipelinesClient.ListDefinitionsAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists all pipelines.")]
    public async Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync()
    {
        return (await _pipelinesClient.ListPipelinesAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a new pipeline.")]
    public async Task<int> CreatePipelineAsync(PipelineCreateOptions options)
    {
        return (await _pipelinesClient.CreatePipelineAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Updates a pipeline.")]
    public async Task<bool> UpdatePipelineAsync(int definitionId, PipelineUpdateOptions options)
    {
        return (await _pipelinesClient.UpdatePipelineAsync(definitionId, options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a pipeline.")]
    public async Task<bool> DeletePipelineAsync(int definitionId)
    {
        return (await _pipelinesClient.DeletePipelineAsync(definitionId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets build logs for a specific build.")]
    public async Task<List<BuildLog>> GetLogsAsync(int buildId)
    {
        return (await _pipelinesClient.GetLogsAsync(buildId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets specific log lines.")]
    public async Task<List<string>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null)
    {
        return (await _pipelinesClient.GetLogLinesAsync(buildId, logId, startLine, endLine)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Downloads console log content.")]
    public async Task<string> DownloadConsoleLogAsync(int buildId)
    {
        return (await _pipelinesClient.DownloadConsoleLogAsync(buildId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets build report metadata.")]
    public async Task<BuildReportMetadata> GetBuildReportAsync(int buildId)
    {
        return (await _pipelinesClient.GetBuildReportAsync(buildId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets changes for a build.")]
    public async Task<List<Change>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false)
    {
        return (await _pipelinesClient.GetChangesAsync(buildId, continuationToken, top, includeSourceChange)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets definition revisions.")]
    public async Task<List<BuildDefinitionRevision>> GetDefinitionRevisionsAsync(int definitionId)
    {
        return (await _pipelinesClient.GetDefinitionRevisionsAsync(definitionId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Updates a build stage.")]
    public async Task<bool> UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false)
    {
        return (await _pipelinesClient.UpdateBuildStageAsync(buildId, stageName, status, forceRetryAllJobs)).EnsureSuccess(_logger);
    }
}
