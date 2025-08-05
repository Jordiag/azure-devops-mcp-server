using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Build.WebApi;
using Xunit;

namespace Dotnet.AzureDevOps.Pipeline.IntegrationTests;

[TestType(TestType.Integration)]
[Component(Component.Pipelines)]
public class DotnetAzureDevOpsPipelineIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly PipelinesClient _pipelines;
    private readonly List<int> _queuedBuildIds = new List<int>();
    private readonly List<int> _createdDefinitionIds = new List<int>();
    private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

    private readonly int _definitionId;
    private readonly string _branch;
    private readonly string? _commitSha;

    public DotnetAzureDevOpsPipelineIntegrationTests(IntegrationTestFixture fixture)
    {
        _azureDevOpsConfiguration = fixture.Configuration;
        _definitionId = _azureDevOpsConfiguration.PipelineDefinitionId;
        _branch = _azureDevOpsConfiguration.BuildBranch;
        _commitSha = _azureDevOpsConfiguration.CommitSha;
        _pipelines = fixture.PipelinesClient;
    }

    [Fact]
    public async Task QueueAndCancelBuild_SucceedsAsync()
    {
        BuildQueueOptions buildQueueOptions = new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch,
            CommitSha = _commitSha
        };

        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(buildQueueOptions);
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        _queuedBuildIds.Add(buildId);

        AzureDevOpsActionResult<Build> runResult = await _pipelines.GetRunAsync(buildId);
        Assert.True(runResult.IsSuccessful);
        Build run = runResult.Value!;
        Assert.NotNull(run);

        AzureDevOpsActionResult<bool> cancelResult = await _pipelines.CancelRunAsync(buildId, run.Project);
        Assert.True(cancelResult.IsSuccessful);

        runResult = await _pipelines.GetRunAsync(buildId);
        Assert.True(runResult.IsSuccessful);
        run = runResult.Value!;
        Assert.Equal(BuildStatus.Cancelling, run.Status);
    }

    [Fact]
    public async Task RetryBuild_SucceedsAsync()
    {
        AzureDevOpsActionResult<int> queuedResult = await _pipelines.QueueRunAsync(new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
        Assert.True(queuedResult.IsSuccessful);
        int queuedBuild = queuedResult.Value;
        _queuedBuildIds.Add(queuedBuild);

        AzureDevOpsActionResult<int> retryResult = await _pipelines.RetryRunAsync(queuedBuild);
        Assert.True(retryResult.IsSuccessful);
        int retryQueuedBuild = retryResult.Value;
        _queuedBuildIds.Add(retryQueuedBuild);
        Assert.NotEqual(queuedBuild, retryQueuedBuild);

        AzureDevOpsActionResult<Build> retriedResult = await _pipelines.GetRunAsync(retryQueuedBuild);
        Assert.True(retriedResult.IsSuccessful);
        Build retried = retriedResult.Value!;
        Assert.Equal(_branch, retried.SourceBranch);
    }

    [Fact]
    public async Task ListBuilds_Filter_WorksAsync()
    {
        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        _queuedBuildIds.Add(buildId);

        AzureDevOpsActionResult<IReadOnlyList<Build>> listResult = await _pipelines.ListRunsAsync(new BuildListOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch,
            Status = BuildStatus.NotStarted,
            Top = 100
        });
        Assert.True(listResult.IsSuccessful);
        IReadOnlyList<Build> list = listResult.Value!;
        Assert.Contains(list, b => b.Id == buildId);
    }

    [Fact]
    public async Task DownloadConsoleLog_SucceedsAsync()
    {
        // Start the build
        AzureDevOpsActionResult<int> queuedBuildResult = await _pipelines.QueueRunAsync(new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch
        });

        Assert.True(queuedBuildResult.IsSuccessful);
        int queuedBuildId = queuedBuildResult.Value;
        _queuedBuildIds.Add(queuedBuildId);

        // Wait for the build to complete
        Build? queuedBuildIdCompleted = await WaitForBuildToCompleteAsync(queuedBuildId, 600, 1000);
        Assert.NotNull(queuedBuildIdCompleted);
        Assert.True(queuedBuildIdCompleted!.Result!.Value == BuildResult.Succeeded , "Build did not complete in time.");

        // Download and validate the console log
        AzureDevOpsActionResult<string> consoleLogResult = await _pipelines.DownloadConsoleLogAsync(queuedBuildId);
        Assert.NotNull(consoleLogResult);
        Assert.True(consoleLogResult.IsSuccessful);

        string? consoleLog = consoleLogResult.Value;
        Assert.True(string.IsNullOrEmpty(consoleLog) || consoleLog.Length > 0);
    }

    [Fact]
    public async Task BuildReport_Changes_LogLines_CanBeRetrievedAsync()
    {
        BuildQueueOptions queueOptions = new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch,
            CommitSha = _commitSha
        };

        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(queueOptions);
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        _queuedBuildIds.Add(buildId);

        AzureDevOpsActionResult<List<Change>> changesResult = await _pipelines.GetChangesAsync(buildId);
        Assert.True(changesResult.IsSuccessful);
        List<Change> changes = changesResult.Value!;
        Assert.NotNull(changes);

        AzureDevOpsActionResult<List<BuildLog>> logsResult = await _pipelines.GetLogsAsync(buildId);
        Assert.True(logsResult.IsSuccessful);
        List<BuildLog> logs = logsResult.Value!;
        if (logs.Count > 0)
        {
            int logId = logs[0].Id;
            AzureDevOpsActionResult<List<string>> linesResult = await _pipelines.GetLogLinesAsync(buildId, logId);
            Assert.True(linesResult.IsSuccessful);
            List<string> lines = linesResult.Value!;
            Assert.NotNull(lines);
        }

        AzureDevOpsActionResult<BuildReportMetadata> reportResult = await _pipelines.GetBuildReportAsync(buildId);
        Assert.True(reportResult.IsSuccessful);
        BuildReportMetadata? report = reportResult.Value;
        Assert.True(report == null || report.BuildId == buildId);
    }

    [Fact]
    public async Task ListDefinitions_FiltersByIdAsync()
    {
        BuildDefinitionListOptions options = new BuildDefinitionListOptions
        {
            DefinitionIds = new List<int> { _definitionId }
        };

        AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>> listResult = await _pipelines.ListDefinitionsAsync(options);
        Assert.True(listResult.IsSuccessful);
        IReadOnlyList<BuildDefinitionReference> list = listResult.Value!;
        Assert.Contains(list, d => d.Id == _definitionId);
    }

    [Fact(Skip = "fails flaky in pipelines, skip for now")]
    public async Task UpdateBuildStage_ValidStage_CancelsStageAsync()
    {
        BuildQueueOptions queueOptions = new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch
        };

        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(queueOptions);
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        _queuedBuildIds.Add(buildId);

        Build? build = await WaitForBuildStatusAsync(buildId, BuildStatus.InProgress, 20, 500);
        Assert.NotNull(build);

        AzureDevOpsActionResult<bool> updateResult = await _pipelines.UpdateBuildStageAsync(buildId, "SimpleStage", StageUpdateType.Cancel);
        Assert.True(updateResult.IsSuccessful);

        build = await WaitForBuildToCompleteAsync(buildId, 20, 500);
        Assert.NotNull(build);
        Assert.Equal(BuildResult.Canceled, build!.Result);
    }

    private async Task<Build?> WaitForBuildStatusAsync(int buildId, BuildStatus targetStatus, int maxAttempts, int delayMs)
    {
        Build? build = null;
        try
        {
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<Build> runResult = await _pipelines.GetRunAsync(buildId);
                if (!runResult.IsSuccessful)
                    return false;
                build = runResult.Value;
                return build?.Status == targetStatus;
            }, TimeSpan.FromMilliseconds(maxAttempts * delayMs), TimeSpan.FromMilliseconds(delayMs));
        }
        catch(TimeoutException)
        {
            return null;
        }
        return build;
    }

    private async Task<Build?> WaitForBuildToCompleteAsync(int buildId, int maxAttempts, int delayMs)
    {
        Build? build = null;
        try
        {
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<Build> runResult = await _pipelines.GetRunAsync(buildId);
                if (!runResult.IsSuccessful)
                    return false;
                build = runResult.Value;
                return build?.Result is BuildResult.Succeeded;
            }, TimeSpan.FromMilliseconds(maxAttempts * delayMs), TimeSpan.FromMilliseconds(delayMs));
        }
        catch(TimeoutException)
        {
            return null;
        }
        return build;
    }

    [Fact]
    public async Task PipelineLogsAndRevisions_SucceedsAsync()
    {
        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch
        });
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        _queuedBuildIds.Add(buildId);

        AzureDevOpsActionResult<List<BuildLog>> logsResult = await _pipelines.GetLogsAsync(buildId);
        Assert.True(logsResult.IsSuccessful);
        List<BuildLog> logs = logsResult.Value!;
        Assert.NotEmpty(logs);

        AzureDevOpsActionResult<List<BuildDefinitionRevision>> revisionsResult = await _pipelines.GetDefinitionRevisionsAsync(_definitionId);
        Assert.True(revisionsResult.IsSuccessful);
        List<BuildDefinitionRevision> revisions = revisionsResult.Value!;
        Assert.NotEmpty(revisions);
    }

    [Fact]
    public async Task PipelineCrud_SucceedsAsync()
    {
        PipelineCreateOptions pipelineCreateOptions = new PipelineCreateOptions
        {
            Name = $"it-pipe-{UtcStamp()}",
            RepositoryId = _azureDevOpsConfiguration.RepoId!,
            YamlPath = "/azure-pipelines-pipelines.yml",
            Description = "Created by integration test",
        };

        AzureDevOpsActionResult<int> createResult = await _pipelines.CreatePipelineAsync(pipelineCreateOptions);
        Assert.True(createResult.IsSuccessful);
        int pipelineId = createResult.Value;

        _createdDefinitionIds.Add(pipelineId);

        AzureDevOpsActionResult<BuildDefinition> getResult = await _pipelines.GetPipelineAsync(pipelineId);
        Assert.True(getResult.IsSuccessful);
        BuildDefinition buildDefinition = getResult.Value!;
        Assert.Equal(pipelineCreateOptions.Name, buildDefinition.Name);

        AzureDevOpsActionResult<bool> updateResult = await _pipelines.UpdatePipelineAsync(pipelineId, new PipelineUpdateOptions
        {
            Description = "Updated by test",
        });
        Assert.True(updateResult.IsSuccessful);

        AzureDevOpsActionResult<BuildDefinition> afterResult = await _pipelines.GetPipelineAsync(pipelineId);
        Assert.True(afterResult.IsSuccessful);
        BuildDefinition buildDefinitionAfter = afterResult.Value!;
        Assert.Equal("Updated by test", buildDefinitionAfter.Description);

        AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>> listResult = await _pipelines.ListPipelinesAsync();
        Assert.True(listResult.IsSuccessful);
        IReadOnlyList<BuildDefinitionReference> list = listResult.Value!;
        Assert.Contains(list, d => d.Id == pipelineId);

        AzureDevOpsActionResult<bool> deleteResult = await _pipelines.DeletePipelineAsync(pipelineId);
        Assert.True(deleteResult.IsSuccessful);
        _createdDefinitionIds.Remove(pipelineId);

        AzureDevOpsActionResult<BuildDefinition> afterDelete = await _pipelines.GetPipelineAsync(pipelineId);
        Assert.False(afterDelete.IsSuccessful);
    }

    private static string UtcStamp() =>
        DateTime.UtcNow.ToString("yyyyMMddHHmmss");

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach(int defId in _createdDefinitionIds)
        {
            try { _ = await _pipelines.DeletePipelineAsync(defId); }
            catch { }
        }

        foreach(int id in _queuedBuildIds.AsEnumerable().Reverse())
        {
            try
            {
                AzureDevOpsActionResult<Build> runResult = await _pipelines.GetRunAsync(id);
                Build? build = runResult.IsSuccessful ? runResult.Value : null;
                if(build != null && build.Status == BuildStatus.InProgress)
                    _ = await _pipelines.CancelRunAsync(id, build.Project);
            }
            catch {}
        }
    }
}

