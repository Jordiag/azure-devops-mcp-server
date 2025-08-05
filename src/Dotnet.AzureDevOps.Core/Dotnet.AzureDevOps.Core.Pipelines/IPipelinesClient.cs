using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines;

public interface IPipelinesClient
{
    Task<AzureDevOpsActionResult<bool>> CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<string?>> DownloadConsoleLogAsync(int buildId);
    Task<AzureDevOpsActionResult<BuildReportMetadata?>> GetBuildReportAsync(int buildId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<List<Change>>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<List<BuildDefinitionRevision>>> GetDefinitionRevisionsAsync(int definitionId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<List<string>>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<List<BuildLog>>> GetLogsAsync(int buildId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<BuildDefinition?>> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Build?>> GetRunAsync(int buildId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListDefinitionsAsync(BuildDefinitionListOptions options, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListPipelinesAsync(CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<Build>>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> RetryRunAsync(int buildId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default);
}

