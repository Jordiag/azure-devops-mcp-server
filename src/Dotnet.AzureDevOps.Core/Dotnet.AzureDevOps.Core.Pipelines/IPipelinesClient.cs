using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines
{
    public interface IPipelinesClient
    {
        Task<int> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default);

        Task<Build?> GetRunAsync(int buildId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Build>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default);

        Task CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default);

        Task<int> RetryRunAsync(int buildId, CancellationToken cancellationToken = default);

        Task<string?> DownloadConsoleLogAsync(int buildId);

        Task<int> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default);

        Task<BuildDefinition?> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync(CancellationToken cancellationToken = default);

        Task UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default);

        Task DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default);
    }
}
