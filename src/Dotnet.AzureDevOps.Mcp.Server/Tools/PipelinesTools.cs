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
    private static PipelinesClient CreateClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new PipelinesClient(organizationUrl, projectName, personalAccessToken, logger);

    [McpServerTool, Description("Queues a new build run.")]
    public static async Task<int> QueueRunAsync(string organizationUrl, string projectName, string personalAccessToken, BuildQueueOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .QueueRunAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a build run by id.")]
    public static async Task<Build> GetRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetRunAsync(buildId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists build runs.")]
    public static async Task<IReadOnlyList<Build>> ListRunsAsync(string organizationUrl, string projectName, string personalAccessToken, BuildListOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListRunsAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Cancels a running build.")]
    public static async Task<bool> CancelRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, TeamProjectReference project, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CancelRunAsync(buildId, project)).EnsureSuccess();
    }

    [McpServerTool, Description("Retries a completed build run.")]
    public static async Task<int> RetryRunAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .RetryRunAsync(buildId)).EnsureSuccess();
    }

    [McpServerTool, Description("Downloads the console log for a build.")]
    public static async Task<string> DownloadConsoleLogAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DownloadConsoleLogAsync(buildId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a new pipeline definition.")]
    public static async Task<int> CreatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, PipelineCreateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreatePipelineAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a pipeline definition.")]
    public static async Task<BuildDefinition> GetPipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetPipelineAsync(definitionId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pipeline definitions.")]
    public static async Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPipelinesAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Updates a pipeline definition.")]
    public static async Task<bool> UpdatePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, PipelineUpdateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .UpdatePipelineAsync(definitionId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a pipeline definition.")]
    public static async Task<bool> DeletePipelineAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeletePipelineAsync(definitionId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists build definitions with advanced filters.")]
    public static async Task<IReadOnlyList<BuildDefinitionReference>> ListDefinitionsAsync(string organizationUrl, string projectName, string personalAccessToken, BuildDefinitionListOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListDefinitionsAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets definition revision history.")]
    public static async Task<List<BuildDefinitionRevision>> GetDefinitionRevisionsAsync(string organizationUrl, string projectName, string personalAccessToken, int definitionId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetDefinitionRevisionsAsync(definitionId)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves build logs.")]
    public static async Task<List<BuildLog>> GetLogsAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetLogsAsync(buildId)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves lines from a build log.")]
    public static async Task<List<string>> GetLogLinesAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, int logId, int? startLine = null, int? endLine = null, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetLogLinesAsync(buildId, logId, startLine, endLine)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets changes associated with a build.")]
    public static async Task<List<Change>> GetChangesAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetChangesAsync(buildId, continuationToken, top, includeSourceChange)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves the build report metadata.")]
    public static async Task<BuildReportMetadata> GetBuildReportAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetBuildReportAsync(buildId)).EnsureSuccess();
    }

    [McpServerTool, Description("Updates the state of a build stage.")]
    public static async Task<bool> UpdateBuildStageAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .UpdateBuildStageAsync(buildId, stageName, status, forceRetryAllJobs)).EnsureSuccess();
    }
}