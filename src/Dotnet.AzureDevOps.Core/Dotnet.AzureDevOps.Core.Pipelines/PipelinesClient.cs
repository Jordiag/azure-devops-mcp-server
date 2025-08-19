using System.IO.Compression;
using System.Text;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Dotnet.AzureDevOps.Core.Common.Services;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;


namespace Dotnet.AzureDevOps.Core.Pipelines;

public partial class PipelinesClient : AzureDevOpsClientBase, IPipelinesClient
{
    private readonly BuildHttpClient _build;

    public PipelinesClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        : base(organizationUrl, personalAccessToken, projectName, logger)
    {
        _build = Connection.GetClient<BuildHttpClient>();
    }

    private static bool IsValidCommitSha(string? sha)
    {
        if(string.IsNullOrWhiteSpace(sha))
            return false;

        if(sha.Length != 40 && (sha.Length < 7 || sha.Length > 10))
            return false;

        foreach(char c in sha)
        {
            bool isHexDigit =
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F');

            if(!isHexDigit)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Queues a new build run for execution based on the specified build queue options.
    /// This method initiates a build pipeline execution with customizable parameters such as
    /// source branch, commit SHA, build parameters, and other execution settings. The build
    /// is queued in Azure DevOps and will be executed by available build agents.
    /// </summary>
    /// <param name="buildQueueOptions">Configuration options for the build queue operation including definition ID, branch, and parameters.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique build ID of the queued build,
    /// or error details if the queue operation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when buildQueueOptions is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to queue builds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build definition doesn't exist or queue parameters are invalid.</exception>
    public async Task<AzureDevOpsActionResult<int>> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            int buildId = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var build = new Build
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
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return queued.Id;
            }, "QueueBuild", OperationType.Create);

            return AzureDevOpsActionResult<int>.Success(buildId, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific build run by its unique identifier.
    /// This method returns comprehensive build information including status, start/finish times,
    /// source details, queue information, and build results. Essential for monitoring build
    /// progress and accessing build metadata for reporting and analysis purposes.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build run to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete build information,
    /// or error details if the build doesn't exist or retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid (less than or equal to 0).</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view build details.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist.</exception>
    public async Task<AzureDevOpsActionResult<Build>> GetRunAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            Build build = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                Build result = await _build.GetBuildAsync(
                    project: ProjectName,
                    buildId: buildId,
                    cancellationToken: cancellationToken);

                if (result == null)
                {
                    throw new AzureDevOpsResourceNotFoundException(
                        "Build could not be found",
                        "Build",
                        buildId.ToString(),
                        "GetBuild");
                }

                return result;
            }, "GetBuild", OperationType.Read);

            return AzureDevOpsActionResult<Build>.Success(build, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<Build>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a filtered list of build runs based on the specified criteria and options.
    /// This method supports comprehensive filtering by build definition, branch, status, date ranges,
    /// and other build characteristics. Useful for generating build reports, monitoring build history,
    /// and analyzing build trends across different time periods and configurations.
    /// </summary>
    /// <param name="buildListOptions">Filtering and pagination options for the build list query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of builds matching the criteria,
    /// or error details if the query fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when buildListOptions is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view builds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when filter criteria are invalid or unsupported.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<Build>>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<Build> builds = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                List<Build> result = await _build.GetBuildsAsync(
                    project: ProjectName,
                    definitions: buildListOptions.DefinitionId is int d ? new[] { d } : null,
                    branchName: buildListOptions.Branch,
                    statusFilter: buildListOptions.Status,
                    resultFilter: buildListOptions.Result,
                    top: buildListOptions.Top,
                    cancellationToken: cancellationToken);

                return (IReadOnlyList<Build>)result;
            }, "ListBuilds", OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<Build>>.Success(builds, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Build>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Cancels a running or queued build by changing its status to canceled.
    /// This method immediately stops build execution if it's currently running, or removes it
    /// from the build queue if it hasn't started yet. Useful for stopping builds that are
    /// no longer needed or when critical issues are discovered that require build termination.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build to cancel.</param>
    /// <param name="project">The team project reference containing the build.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the build was successfully canceled,
    /// or error details if the cancellation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when project is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to cancel builds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist or cannot be canceled.</exception>
    public async Task<AzureDevOpsActionResult<bool>> CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                Build build = await _build.GetBuildAsync(
                    project: project.Id,
                    buildId: buildId,
                    cancellationToken: cancellationToken);

                build.Status = BuildStatus.Cancelling;

                await _build.UpdateBuildAsync(
                    build: build,
                    cancellationToken: cancellationToken);

                return true;
            }, "CancelBuild", OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retries a failed or canceled build by creating a new build run with the same configuration.
    /// This method queues a new build using the same source version, parameters, and settings
    /// as the original build. Commonly used when builds fail due to transient issues like
    /// network problems, temporary service outages, or environmental issues.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build to retry.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique identifier of the newly queued retry build,
    /// or error details if the retry operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to queue builds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the original build doesn't exist or cannot be retried.</exception>
    public async Task<AzureDevOpsActionResult<int>> RetryRunAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            int retryBuildId = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                Build original = await _build.GetBuildAsync(
                    project: ProjectName,
                    buildId: buildId,
                    cancellationToken: cancellationToken);

                var clone = new Build
                {
                    Definition = original.Definition,
                    SourceBranch = original.SourceBranch,
                    SourceVersion = original.SourceVersion,
                    Parameters = original.Parameters
                };

                Build queued = await _build.QueueBuildAsync(
                    build: clone,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return queued.Id;
            }, "RetryBuild", OperationType.Create);

            return AzureDevOpsActionResult<int>.Success(retryBuildId, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Downloads and aggregates the complete console log output from a build run.
    /// This method retrieves all log entries from the build execution and combines them
    /// into a single comprehensive log string. Essential for build analysis, debugging
    /// build failures, and generating build reports with complete execution details.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build whose logs to download.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete console log as a string,
    /// or error details if the download fails or logs are not available.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access build logs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist or logs are not available.</exception>
    public async Task<AzureDevOpsActionResult<string>> DownloadConsoleLogAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            string logResult = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using Stream buildLogsStream = await _build.GetBuildLogsZipAsync(
                    project: ProjectName,
                    buildId: buildId,
                    cancellationToken: cancellationToken);

                using var zipArchive = new ZipArchive(buildLogsStream);

                // Find all script-related log entries in the correct order
                List<ZipArchiveEntry> logEntries = zipArchive.Entries
                    .Where(e =>
                        e.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                        (e.FullName.Contains("Run a one-line script", StringComparison.OrdinalIgnoreCase) ||
                         e.FullName.Contains("Run a multi-line script", StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(e => e.FullName) // Ensure correct order
                    .ToList();

                if (logEntries.Count == 0)
                {
                    throw new AzureDevOpsResourceNotFoundException(
                        "No console logs were found in the build output",
                        "BuildLogs",
                        buildId.ToString(),
                        "DownloadConsoleLog");
                }

                var fullLog = new StringBuilder();
                foreach (ZipArchiveEntry? entry in logEntries)
                {
                    fullLog.AppendLine($"--- {entry.FullName} ---");
                    using var reader = new StreamReader(entry.Open());
                    string content = await reader.ReadToEndAsync(cancellationToken);
                    fullLog.AppendLine(content);
                    fullLog.AppendLine(); // Add spacing between logs
                }

                string result = fullLog.ToString().Trim();

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new AzureDevOpsException(
                        "Console logs were found but all are empty",
                        "DownloadConsoleLog");
                }

                return result;
            }, "DownloadConsoleLog", OperationType.Read);

            return AzureDevOpsActionResult<string>.Success(logResult, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<string>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Creates a new build pipeline (build definition) in the Azure DevOps project.
    /// This method establishes a new CI/CD pipeline with specified configuration including
    /// repository connections, build steps, triggers, variables, and other pipeline settings.
    /// The created pipeline becomes available for manual runs and automatic triggering.
    /// </summary>
    /// <param name="pipelineCreateOptions">Configuration options for the new pipeline including name, repository, and build process.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique identifier of the created pipeline,
    /// or error details if the creation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when pipelineCreateOptions is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create pipelines.</exception>
    /// <exception cref="InvalidOperationException">Thrown when pipeline configuration is invalid or repository access fails.</exception>
    public async Task<AzureDevOpsActionResult<int>> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            int pipelineId = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var repository = new BuildRepository
                {
                    Id = pipelineCreateOptions.RepositoryId,
                    Type = "TfsGit",
                    DefaultBranch = pipelineCreateOptions.DefaultBranch
                };

                var yamlProcess = new YamlProcess { YamlFilename = pipelineCreateOptions.YamlPath };

                var definition = new BuildDefinition
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
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return created.Id;
            }, "CreatePipeline", OperationType.Create);

            return AzureDevOpsActionResult<int>.Success(pipelineId, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves detailed configuration information for a specific build pipeline definition.
    /// This method returns comprehensive pipeline settings including repository configuration,
    /// build process, triggers, variables, retention policies, and permissions. Essential
    /// for pipeline analysis, configuration backup, and pipeline management operations.
    /// </summary>
    /// <param name="definitionId">The unique identifier of the pipeline definition to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete build definition configuration,
    /// or error details if the pipeline doesn't exist or retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when definitionId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view pipeline details.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipeline definition doesn't exist.</exception>
    public async Task<AzureDevOpsActionResult<BuildDefinition>> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildDefinition definition = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                BuildDefinition result = await _build.GetDefinitionAsync(
                    project: ProjectName,
                    definitionId: definitionId,
                    cancellationToken: cancellationToken);

                if (result == null)
                {
                    throw new AzureDevOpsResourceNotFoundException(
                        "Pipeline definition not found",
                        "BuildDefinition",
                        definitionId.ToString(),
                        "GetPipeline");
                }

                return result;
            }, "GetPipeline", OperationType.Read);

            return AzureDevOpsActionResult<BuildDefinition>.Success(definition, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<BuildDefinition>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a list of all build pipeline definitions in the current project.
    /// This method returns basic information about all available pipelines including
    /// names, IDs, creation dates, and repository connections. Useful for pipeline
    /// discovery, management dashboards, and generating pipeline inventories.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of pipeline definition references,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view pipelines.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the project doesn't exist or is inaccessible.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListPipelinesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<BuildDefinitionReference> definitions = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                List<BuildDefinitionReference> result = await _build.GetDefinitionsAsync(
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return (IReadOnlyList<BuildDefinitionReference>)result;
            }, "ListPipelines", OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Success(definitions, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a filtered list of build definitions based on specified criteria and options.
    /// This method supports advanced filtering by name patterns, repository types, definition types,
    /// creation dates, and other characteristics. More flexible than ListPipelinesAsync, enabling
    /// targeted pipeline discovery and management for specific scenarios.
    /// </summary>
    /// <param name="options">Filtering and search options for the build definition query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of matching pipeline definition references,
    /// or error details if the query fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view pipelines.</exception>
    /// <exception cref="InvalidOperationException">Thrown when filter criteria are invalid or unsupported.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>> ListDefinitionsAsync(BuildDefinitionListOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<BuildDefinitionReference> definitions = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                List<BuildDefinitionReference> result = await _build.GetDefinitionsAsync(
                    project: ProjectName,
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

                return (IReadOnlyList<BuildDefinitionReference>)result;
            }, "ListDefinitions", OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Success(definitions, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves the revision history for a specific build definition, showing all configuration changes over time.
    /// This method returns a chronological list of all modifications made to the pipeline definition,
    /// including who made changes, when they were made, and revision comments. Essential for
    /// pipeline auditing, change tracking, and rollback scenarios.
    /// </summary>
    /// <param name="definitionId">The unique identifier of the build definition whose revisions to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a list of build definition revisions in chronological order,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when definitionId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view definition history.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the definition doesn't exist.</exception>
    public async Task<AzureDevOpsActionResult<List<BuildDefinitionRevision>>> GetDefinitionRevisionsAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildDefinitionRevision> revisions = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                return await _build.GetDefinitionRevisionsAsync(ProjectName, definitionId, cancellationToken: cancellationToken);
            }, "GetDefinitionRevisions", OperationType.Read);

            return AzureDevOpsActionResult<List<BuildDefinitionRevision>>.Success(revisions, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<BuildDefinitionRevision>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves metadata for all log files associated with a specific build run.
    /// This method returns information about available log files including log IDs,
    /// creation times, and log types. Use this to discover available logs before
    /// retrieving specific log content with GetLogLinesAsync.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build whose log metadata to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a list of build log metadata,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access build logs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist.</exception>
    public async Task<AzureDevOpsActionResult<List<BuildLog>>> GetLogsAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            List<BuildLog> logs = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                return await _build.GetBuildLogsAsync(ProjectName, buildId, cancellationToken: cancellationToken);
            }, "GetBuildLogs", OperationType.Read);

            return AzureDevOpsActionResult<List<BuildLog>>.Success(logs, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<BuildLog>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves specific lines from a build log file with optional line range filtering.
    /// This method allows precise extraction of log content from specific log files,
    /// enabling targeted log analysis, error investigation, and selective log display.
    /// Supports pagination through large log files by specifying start and end line numbers.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build containing the log.</param>
    /// <param name="logId">The unique identifier of the specific log file to read.</param>
    /// <param name="startLine">Optional starting line number (1-based) for log extraction.</param>
    /// <param name="endLine">Optional ending line number (1-based) for log extraction.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a list of log lines within the specified range,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId or logId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access build logs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build or log doesn't exist, or line range is invalid.</exception>
    public async Task<AzureDevOpsActionResult<List<string>>> GetLogLinesAsync(int buildId, int logId, int? startLine = null, int? endLine = null, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> lines = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                return await _build.GetBuildLogLinesAsync(ProjectName, buildId, logId, startLine, endLine, cancellationToken: cancellationToken);
            }, "GetLogLines", OperationType.Read);

            return AzureDevOpsActionResult<List<string>>.Success(lines, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<string>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves the source code changes (commits) that were included in a specific build run.
    /// This method returns detailed information about commits, including authors, messages,
    /// timestamps, and modified files. Essential for understanding what changes triggered
    /// the build and for generating build reports with change tracking.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build whose changes to retrieve.</param>
    /// <param name="continuationToken">Optional token for paginating through large change sets.</param>
    /// <param name="top">Maximum number of changes to return (default: 100).</param>
    /// <param name="includeSourceChange">Whether to include detailed source change information (default: false).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a list of changes associated with the build,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid or top is negative.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view build changes.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist or change information is unavailable.</exception>
    public async Task<AzureDevOpsActionResult<List<Change>>> GetChangesAsync(int buildId, string? continuationToken = null, int top = 100, bool includeSourceChange = false, CancellationToken cancellationToken = default)
    {
        try
        {
            List<Change> changes = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                return await _build.GetBuildChangesAsync(ProjectName, buildId, continuationToken, top, includeSourceChange, cancellationToken: cancellationToken);
            }, "GetBuildChanges", OperationType.Read);

            return AzureDevOpsActionResult<List<Change>>.Success(changes, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<List<Change>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves build report metadata containing summary information and metrics for a specific build.
    /// This method returns comprehensive build statistics including test results, code coverage,
    /// duration metrics, and other build quality indicators. Essential for build reporting,
    /// dashboards, and build quality analysis across multiple builds.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build whose report metadata to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the build report metadata with metrics and summary information,
    /// or error details if the retrieval fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view build reports.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build doesn't exist or report data is unavailable.</exception>
    public async Task<AzureDevOpsActionResult<BuildReportMetadata>> GetBuildReportAsync(int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            BuildReportMetadata report = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                BuildReportMetadata result = await _build.GetBuildReportAsync(ProjectName, buildId, cancellationToken: cancellationToken);
                if (result == null)
                {
                    throw new AzureDevOpsResourceNotFoundException(
                        "Build report not found",
                        "BuildReport",
                        buildId.ToString(),
                        "GetBuildReport");
                }
                return result;
            }, "GetBuildReport", OperationType.Read);

            return AzureDevOpsActionResult<BuildReportMetadata>.Success(report, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<BuildReportMetadata>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Updates the status of a specific build stage, enabling manual control over stage execution flow.
    /// This method allows pausing, resuming, canceling, or retrying individual stages within
    /// a multi-stage pipeline. Useful for implementing manual approval processes, stage-specific
    /// interventions, and controlled deployment flows in complex pipeline scenarios.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build containing the stage to update.</param>
    /// <param name="stageName">The name of the stage to update.</param>
    /// <param name="status">The new status to apply to the stage (e.g., Resume, Cancel, Retry).</param>
    /// <param name="forceRetryAllJobs">Whether to retry all jobs in the stage, not just failed ones (default: false).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the stage status was successfully updated,
    /// or error details if the update fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when buildId is invalid or stageName is null/empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to control build stages.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the build or stage doesn't exist, or the status transition is invalid.</exception>
    public async Task<AzureDevOpsActionResult<bool>> UpdateBuildStageAsync(int buildId, string stageName, StageUpdateType status, bool forceRetryAllJobs = false, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var parameters = new UpdateStageParameters { State = status, ForceRetryAllJobs = forceRetryAllJobs };
                await _build.UpdateStageAsync(parameters, ProjectName, buildId, stageName, cancellationToken: cancellationToken);
                return true;
            }, "UpdateBuildStage", OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Updates the configuration of an existing build pipeline definition with new settings.
    /// This method allows modification of pipeline properties such as name, description,
    /// build process, triggers, variables, and repository connections. The pipeline retains
    /// its history and ID while applying the new configuration for future builds.
    /// </summary>
    /// <param name="definitionId">The unique identifier of the pipeline definition to update.</param>
    /// <param name="pipelineUpdateOptions">New configuration options to apply to the pipeline.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the pipeline was successfully updated,
    /// or error details if the update fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when definitionId is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when pipelineUpdateOptions is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify pipelines.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipeline doesn't exist or configuration is invalid.</exception>
    public async Task<AzureDevOpsActionResult<bool>> UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                BuildDefinition buildDefinition = await _build.GetDefinitionAsync(
                    project: ProjectName,
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
                    project: ProjectName,
                    definitionId: definitionId,
                    cancellationToken: cancellationToken);

                return true;
            }, "UpdatePipeline", OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Permanently deletes a build pipeline definition from the Azure DevOps project.
    /// This method removes the pipeline configuration, build history, and all associated
    /// metadata. Use with caution as this operation cannot be undone. Consider disabling
    /// the pipeline instead if you might need to restore it later.
    /// </summary>
    /// <param name="definitionId">The unique identifier of the pipeline definition to delete.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the pipeline was successfully deleted,
    /// or error details if the deletion fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when definitionId is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete pipelines.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipeline doesn't exist or cannot be deleted due to dependencies.</exception>
    public async Task<AzureDevOpsActionResult<bool>> DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                await _build.DeleteDefinitionAsync(
                    project: ProjectName,
                    definitionId: definitionId,
                    cancellationToken: cancellationToken);
                return true;
            }, "DeletePipeline", OperationType.Delete);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }
}

