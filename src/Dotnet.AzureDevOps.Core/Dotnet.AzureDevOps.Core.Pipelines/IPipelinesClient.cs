using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines
{
    public interface IPipelinesClient
    {
        Task CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default);
        Task<int> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default);
        Task DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default);
        Task<string?> DownloadConsoleLogAsync(int buildId);
        Task<BuildReportMetadata?> GetBuildReportAsync(int buildId, CancellationToken cancellationToken = default);
        Task<List<Change>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false, CancellationToken cancellationToken = default);
        Task<List<BuildDefinitionRevision>> GetDefinitionRevisionsAsync(int definitionId, CancellationToken cancellationToken = default);
        Task<List<string>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null, CancellationToken cancellationToken = default);
        Task<List<BuildLog>> GetLogsAsync(int buildId, CancellationToken cancellationToken = default);
        Task<BuildDefinition?> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default);
        Task<Build?> GetRunAsync(int buildId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BuildDefinitionReference>> ListDefinitionsAsync(BuildDefinitionListOptions options, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Build>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default);
        Task<int> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default);
        Task<int> RetryRunAsync(int buildId, CancellationToken cancellationToken = default);
        Task UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false, CancellationToken cancellationToken = default);
        Task UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default);
    }
}