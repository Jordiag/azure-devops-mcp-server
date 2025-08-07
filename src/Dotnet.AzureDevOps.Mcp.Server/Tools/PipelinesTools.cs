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

    [McpServerTool, Description("Queues a new build run (execution) of an Azure DevOps pipeline. This triggers a new build/CI/CD process based on a pipeline definition. Requires definition ID, target branch, and optional commit SHA. Returns the unique build ID that can be used to track progress, get logs, or cancel the run.")]
    public async Task<int> QueueRunAsync(BuildQueueOptions options) =>
        (await _pipelinesClient.QueueRunAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific build run using its build ID. Returns comprehensive build details including status (NotStarted, InProgress, Completed), result (Succeeded, Failed, Canceled), start/finish times, source branch, commit SHA, and associated pipeline definition.")]
    public async Task<Build> GetRunAsync(int buildId) =>
        (await _pipelinesClient.GetRunAsync(buildId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists build runs based on filtering criteria such as pipeline definition, branch, build status, date range, or build result. Supports pagination with 'top' parameter. Useful for finding recent builds, failed builds, or builds for specific branches. Returns build summary information.")]
    public async Task<IReadOnlyList<Build>> ListRunsAsync(BuildListOptions options) =>
        (await _pipelinesClient.ListRunsAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Cancels a currently running or queued build in Azure DevOps. The build status will change to 'Cancelling' and then 'Canceled'. Cannot cancel already completed builds. Useful for stopping builds that are taking too long or were triggered by mistake. Returns true if cancellation was initiated successfully.")]
    public async Task<bool> CancelRunAsync(int buildId, TeamProjectReference project) =>
        (await _pipelinesClient.CancelRunAsync(buildId, project)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new build run by retrying/re-running an existing completed build with the same parameters (branch, commit, variables). This is useful for re-running failed builds or reproducing issues. Returns the new build ID of the retried run.")]
    public async Task<int> RetryRunAsync(int buildId) =>
        (await _pipelinesClient.RetryRunAsync(buildId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves a complete build pipeline definition (build template) by its ID. Returns detailed configuration including build steps, triggers, variables, agent pool settings, and repository configuration. This is the blueprint that defines how builds are executed.")]
    public async Task<BuildDefinition> GetPipelineAsync(int definitionId) =>
        (await _pipelinesClient.GetPipelineAsync(definitionId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists build pipeline definitions (templates) based on filtering criteria such as name, repository, folder path, or definition type. Returns summary information about available pipelines that can be used to trigger builds. Supports filtering and pagination.")]
    public async Task<IReadOnlyList<BuildDefinitionReference>> ListDefinitionsAsync(BuildDefinitionListOptions options) =>
        (await _pipelinesClient.ListDefinitionsAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all build pipeline definitions in the Azure DevOps project. This provides a complete overview of all available CI/CD pipelines without filtering. Returns basic information about each pipeline including name, ID, and path.")]
    public async Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync() =>
        (await _pipelinesClient.ListPipelinesAsync()).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new build pipeline definition from a YAML file in a Git repository. Requires pipeline name, repository ID, YAML file path, and optional description. This establishes a new CI/CD pipeline that can be triggered manually or by events. Returns the new pipeline definition ID.")]
    public async Task<int> CreatePipelineAsync(PipelineCreateOptions options) =>
        (await _pipelinesClient.CreatePipelineAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Updates an existing build pipeline definition with new settings such as name, description, variables, or triggers. This modifies the pipeline template but does not affect running builds. Returns true if the update was successful.")]
    public async Task<bool> UpdatePipelineAsync(int definitionId, PipelineUpdateOptions options) =>
        (await _pipelinesClient.UpdatePipelineAsync(definitionId, options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a build pipeline definition from Azure DevOps. This removes the pipeline template and all its history. Running builds will continue but no new builds can be queued. This action cannot be undone. Returns true if deletion was successful.")]
    public async Task<bool> DeletePipelineAsync(int definitionId) =>
        (await _pipelinesClient.DeletePipelineAsync(definitionId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves all log files generated during a specific build run. Each build typically produces multiple log files for different build steps, tasks, or stages. Returns metadata about available logs including log IDs, types, and creation times. Use GetLogLinesAsync to retrieve actual log content.")]
    public async Task<List<BuildLog>> GetLogsAsync(int buildId) =>
        (await _pipelinesClient.GetLogsAsync(buildId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves specific lines from a build log file. Allows reading partial log content by specifying start and end line numbers, which is useful for large logs or focusing on specific errors. If no line range is specified, returns the entire log content as individual lines.")]
    public async Task<List<string>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null) =>
        (await _pipelinesClient.GetLogLinesAsync(buildId, logId, startLine, endLine)).EnsureSuccess(_logger);

    [McpServerTool, Description("Downloads the complete console log output from a build run as a single text string. This provides the full build output including all steps, tasks, and error messages. Useful for comprehensive build analysis or debugging build failures.")]
    public async Task<string> DownloadConsoleLogAsync(int buildId) =>
        (await _pipelinesClient.DownloadConsoleLogAsync(buildId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves build report metadata containing summary information about test results, code coverage, and other build artifacts. May return null if no report data is available for the build. Useful for getting build quality metrics and test result summaries.")]
    public async Task<BuildReportMetadata> GetBuildReportAsync(int buildId) =>
        (await _pipelinesClient.GetBuildReportAsync(buildId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the source code changes (commits) that triggered or are included in a specific build. Returns commit information including SHA, author, message, and changed files. Supports pagination and filtering options. Useful for understanding what code changes caused build issues.")]
    public async Task<List<Change>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false) =>
        (await _pipelinesClient.GetChangesAsync(buildId, continuationToken, top, includeSourceChange)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the revision history of a build pipeline definition, showing how the pipeline configuration has changed over time. Each revision includes the modification date, author, and changes made. Useful for auditing pipeline changes or reverting to previous configurations.")]
    public async Task<List<BuildDefinitionRevision>> GetDefinitionRevisionsAsync(int definitionId) =>
        (await _pipelinesClient.GetDefinitionRevisionsAsync(definitionId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Updates the status of a specific stage in a running build pipeline. Can be used to cancel, retry, or force retry all jobs in a stage. Useful for managing multi-stage pipelines where individual stages may need intervention. Returns true if the stage update was successful.")]
    public async Task<bool> UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false) =>
        (await _pipelinesClient.UpdateBuildStageAsync(buildId, stageName, status, forceRetryAllJobs)).EnsureSuccess(_logger);
}
