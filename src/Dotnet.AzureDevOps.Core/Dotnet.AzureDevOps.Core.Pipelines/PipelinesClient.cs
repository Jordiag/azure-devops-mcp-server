using System.IO;
using System.IO.Compression;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines;

public partial class PipelinesClient : IPipelinesClient
{
    private readonly string _projectName;
    private readonly BuildHttpClient _build;

    public PipelinesClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName;

        VssConnection connection = new VssConnection(new Uri(organizationUrl),
            new VssBasicCredential(string.Empty, personalAccessToken));
        _build = connection.GetClient<BuildHttpClient>();
    }

    private static bool IsValidCommitSha(string? sha)
    {
        if (string.IsNullOrWhiteSpace(sha))
            return false;

        if (sha.Length != 40 && (sha.Length < 7 || sha.Length > 10))
            return false;

        foreach (char c in sha)
        {
            bool isHexDigit =
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F');

            if (!isHexDigit)
                return false;
        }

        return true;
    }

    public async Task<AzureDevOpsActionResult<int>> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            Build build = new Build
            {
                Definition = new DefinitionReference { Id = buildQueueOptions.DefinitionId },
                SourceBranch = buildQueueOptions.Branch
            };

            if (IsValidCommitSha(buildQueueOptions.CommitSha))
                build.SourceVersion = buildQueueOptions.CommitSha!;

            if (buildQueueOptions.Parameters is { Count: > 0 })
                build.Parameters = System.Text.Json.JsonSerializer.Serialize(buildQueueOptions.Parameters);

            Build queued = await _build.QueueBuildAsync(
                build: build,
                project: _projectName,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<int>.Success(queued.Id);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<Build>> GetRunAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            Build build = await _build.GetBuildAsync(
                project: _projectName,
                buildId: buildId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<Build>.Success(build);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<Build>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<Build>>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            List<Build> builds = await _build.GetBuildsAsync(
                project: _projectName,
                definitions: buildListOptions.DefinitionId is int d ? new[] { d } : null,
                branchName: buildListOptions.Branch,
                statusFilter: buildListOptions.Status,
                resultFilter: buildListOptions.Result,
                top: buildListOptions.Top,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<IReadOnlyList<Build>>.Success(builds);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Build>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default)
    {
        try
        {
            Build build = await _build.GetBuildAsync(
                project: project.Id,
                buildId: buildId,
                cancellationToken: cancellationToken);

            build.Status = BuildStatus.Cancelling;

            await _build.UpdateBuildAsync(
                build: build,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<int>> RetryRunAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            Build original = await _build.GetBuildAsync(
                project: _projectName,
                buildId: buildId,
                cancellationToken: cancellationToken);

            Build clone = new Build
            {
                Definition = original.Definition,
                SourceBranch = original.SourceBranch,
                SourceVersion = original.SourceVersion,
                Parameters = original.Parameters
            };

            Build queued = await _build.QueueBuildAsync(
                build: clone,
                project: _projectName,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<int>.Success(queued.Id);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<string?>> DownloadConsoleLogAsync(int buildId)
    {
        try
        {
            using Stream buildLogsStream = await _build.GetBuildLogsZipAsync(
                project: _projectName,
                buildId: buildId);
            using ZipArchive zipArchive = new ZipArchive(buildLogsStream);
            ZipArchiveEntry? logEntry = zipArchive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".log"));
            if (logEntry == null)
                return AzureDevOpsActionResult<string?>.Success(null);

            using StreamReader logReader = new StreamReader(logEntry.Open());
            string logContent = await logReader.ReadToEndAsync();
            return AzureDevOpsActionResult<string?>.Success(logContent);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<string?>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<int>> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildRepository repository = new BuildRepository
            {
                Id = pipelineCreateOptions.RepositoryId,
                Type = "TfsGit",
                DefaultBranch = pipelineCreateOptions.DefaultBranch
            };

            YamlProcess yamlProcess = new YamlProcess { YamlFilename = pipelineCreateOptions.YamlPath };

            BuildDefinition definition = new BuildDefinition
            {
                Name = pipelineCreateOptions.Name,
                Repository = repository,
                Process = yamlProcess,
                Path = "\\",
                Description = pipelineCreateOptions.Description,
                QueueStatus = DefinitionQueueStatus.Enabled
            };

            BuildDefinition created = await _build.CreateDefinitionAsync(
                definition: definition,
                project: _projectName,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<int>.Success(created.Id);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<BuildDefinition>> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildDefinition definition = await _build.GetDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<BuildDefinition>.Success(definition);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<BuildDefinition>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListPipelinesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildDefinitionReference> definitions = await _build.GetDefinitionsAsync(
                project: _projectName,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Success(definitions);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListDefinitionsAsync(BuildDefinitionListOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildDefinitionReference> definitions = await _build.GetDefinitionsAsync(
                project: _projectName,
                name: options.Name,
                repositoryId: options.RepositoryId,
                repositoryType: options.RepositoryType,
                queryOrder: options.QueryOrder,
                top: options.Top,
                continuationToken: options.ContinuationToken,
                minMetricsTimeInUtc: options.MinMetricsTimeInUtc,
                definitionIds: options.DefinitionIds?.ToArray(),
                path: options.Path,
                builtAfter: options.BuiltAfter,
                notBuiltAfter: options.NotBuiltAfter,
                includeLatestBuilds: options.IncludeLatestBuilds,
                taskIdFilter: options.TaskIdFilter,
                processType: options.ProcessType,
                yamlFilename: options.YamlFilename,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Success(definitions);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<List<BuildDefinitionRevision>>> GetDefinitionRevisionsAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildDefinitionRevision> revisions = await _build.GetDefinitionRevisionsAsync(_projectName, definitionId, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<List<BuildDefinitionRevision>>.Success(revisions);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<BuildDefinitionRevision>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<List<BuildLog>>> GetLogsAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildLog> logs = await _build.GetBuildLogsAsync(_projectName, buildId, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<List<BuildLog>>.Success(logs);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<BuildLog>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<List<string>>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> lines = await _build.GetBuildLogLinesAsync(_projectName, buildId, logId, startLine, endLine, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<List<string>>.Success(lines);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<string>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<List<Change>>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false, CancellationToken cancellationToken = default)
    {
        try
        {
            List<Change> changes = await _build.GetBuildChangesAsync(_projectName, buildId, continuationToken, top, includeSourceChange, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<List<Change>>.Success(changes);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<Change>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<BuildReportMetadata?>> GetBuildReportAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildReportMetadata? report = await _build.GetBuildReportAsync(_projectName, buildId, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<BuildReportMetadata?>.Success(report);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<BuildReportMetadata?>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false, CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateStageParameters parameters = new UpdateStageParameters { State = status, ForceRetryAllJobs = forceRetryAllJobs };
            await _build.UpdateStageAsync(parameters, _projectName, buildId, stageName, cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildDefinition buildDefinition = await _build.GetDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

            if (pipelineUpdateOptions.Name is { Length: > 0 })
                buildDefinition.Name = pipelineUpdateOptions.Name;
            if (pipelineUpdateOptions.Description is { Length: > 0 })
                buildDefinition.Description = pipelineUpdateOptions.Description;
            if (pipelineUpdateOptions.DefaultBranch is { Length: > 0 })
                buildDefinition.Repository.DefaultBranch = pipelineUpdateOptions.DefaultBranch;
            if (pipelineUpdateOptions.YamlPath is { Length: > 0 } && buildDefinition.Process is YamlProcess yamlProcess)
                yamlProcess.YamlFilename = pipelineUpdateOptions.YamlPath;

            await _build.UpdateDefinitionAsync(
                definition: buildDefinition,
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _build.DeleteDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }
}

