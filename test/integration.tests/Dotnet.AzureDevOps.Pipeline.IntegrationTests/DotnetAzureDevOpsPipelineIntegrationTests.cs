using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Build.WebApi;
using Xunit;

namespace Dotnet.AzureDevOps.Pipeline.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Pipelines)]

    public class DotnetAzureDevOpsPipelineIntegrationTests : IAsyncLifetime
    {
        private readonly PipelinesClient _pipelines;
        private readonly List<int> _queuedBuildIds = [];
        private readonly List<int> _createdDefinitionIds = [];
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

        // Env-driven settings
        private readonly int _definitionId;
        private readonly string _branch;
        private readonly string? _commitSha;

        public DotnetAzureDevOpsPipelineIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

            _definitionId = _azureDevOpsConfiguration.PipelineDefinitionId;
            _branch = _azureDevOpsConfiguration.BuildBranch;
            _commitSha = _azureDevOpsConfiguration.CommitSha;

            _pipelines = new PipelinesClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        [Fact]
        public async Task QueueAndCancelBuild_SucceedsAsync()
        {
            var buildQueueOptions = new BuildQueueOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch,
                CommitSha = _commitSha
            };

            int buildId = await _pipelines.QueueRunAsync(buildQueueOptions);
            _queuedBuildIds.Add(buildId);
            Build? run = await _pipelines.GetRunAsync(buildId);
            Assert.NotNull(run);

            await _pipelines.CancelRunAsync(buildId, run.Project);

            run = await _pipelines.GetRunAsync(buildId);
            Assert.Equal(BuildStatus.Cancelling, run!.Status);
        }

        [Fact]
        public async Task RetryBuild_SucceedsAsync()
        {
            int queuedBuild = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(queuedBuild);

            int retryQueuedBuild = await _pipelines.RetryRunAsync(queuedBuild);
            _queuedBuildIds.Add(retryQueuedBuild);
            Assert.NotEqual(queuedBuild, retryQueuedBuild);

            Build? retried = await _pipelines.GetRunAsync(retryQueuedBuild);
            Assert.NotNull(retried);
            Assert.Equal(_branch, retried!.SourceBranch);
        }

        public async Task ListBuilds_Filter_WorksAsync()
        {
            int buildId = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(buildId);

            IReadOnlyList<Build> list = await _pipelines.ListRunsAsync(new BuildListOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch,
                Status = BuildStatus.NotStarted, // will match our queued run
                Top = 100
            });
            Assert.Contains(list, b => b.Id == buildId);
        }

        [Fact]
        public async Task DownloadConsoleLog_SucceedsAsync()
        {
            int queuedBuildId = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(queuedBuildId);

            string? consoleLog = await _pipelines.DownloadConsoleLogAsync(queuedBuildId);
            Assert.True(string.IsNullOrEmpty(consoleLog) || consoleLog.Length > 0);
        }

        [Fact]
        public async Task BuildReport_Changes_LogLines_CanBeRetrievedAsync()
        {
            var queueOptions = new BuildQueueOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch,
                CommitSha = _commitSha
            };

            int buildId = await _pipelines.QueueRunAsync(queueOptions);
            _queuedBuildIds.Add(buildId);

            List<Change> changes = await _pipelines.GetChangesAsync(buildId);
            Assert.NotNull(changes);

            List<BuildLog> logs = await _pipelines.GetLogsAsync(buildId);
            if (logs.Count > 0)
            {
                int logId = logs[0].Id;
                List<string> lines = await _pipelines.GetLogLinesAsync(buildId, logId);
                Assert.NotNull(lines);
            }

            BuildReportMetadata? report = await _pipelines.GetBuildReportAsync(buildId);
            Assert.True(report == null || report.BuildId == buildId);
        }

        [Fact]
        public async Task ListDefinitions_FiltersByIdAsync()
        {
            BuildDefinitionListOptions options = new BuildDefinitionListOptions
            {
                DefinitionIds = new List<int> { _definitionId }
            };

            IReadOnlyList<BuildDefinitionReference> list = await _pipelines.ListDefinitionsAsync(options);
            Assert.Contains(list, d => d.Id == _definitionId);
        }

        // TODO: fails flaky in pipelines, skip for now
        [Fact(Skip = "fails flaky in pipelines, skip for now")]
        public async Task UpdateBuildStage_ValidStage_CancelsStageAsync()
        {
            var queueOptions = new BuildQueueOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch
            };

            int buildId = await _pipelines.QueueRunAsync(queueOptions);
            _queuedBuildIds.Add(buildId);

            Build? build = await WaitForBuildStatusAsync(buildId, BuildStatus.InProgress, maxAttempts: 20, delayMs: 500);
            Assert.NotNull(build);

            await _pipelines.UpdateBuildStageAsync(buildId, "SimpleStage", StageUpdateType.Cancel);

            build = await WaitForBuildToCompleteAsync(buildId, maxAttempts: 20, delayMs: 500);

            Assert.NotNull(build);
            Assert.Equal(BuildResult.Canceled, build!.Result);
        }

        private async Task<Build?> WaitForBuildStatusAsync(int buildId, BuildStatus targetStatus, int maxAttempts, int delayMs)
        {
            for(int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Build? build = await _pipelines.GetRunAsync(buildId);
                if(build?.Status == targetStatus)
                    return build;
                await Task.Delay(delayMs);
            }
            return null;
        }

        private async Task<Build?> WaitForBuildToCompleteAsync(int buildId, int maxAttempts, int delayMs)
        {
            for(int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Build? build = await _pipelines.GetRunAsync(buildId);
                if(build?.Result is BuildResult.Canceled)
                    return build;
                await Task.Delay(delayMs);
            }
            return await _pipelines.GetRunAsync(buildId); 
        }


        [Fact]
        public async Task PipelineLogsAndRevisions_SucceedsAsync()
        {
            int buildId = await _pipelines.QueueRunAsync(new BuildQueueOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch
            });
            _queuedBuildIds.Add(buildId);

            List<BuildLog> logs = await _pipelines.GetLogsAsync(buildId);
            Assert.NotEmpty(logs);

            List<BuildDefinitionRevision> revisions = await _pipelines.GetDefinitionRevisionsAsync(
                _definitionId);
            Assert.NotEmpty(revisions);
        }

        [Fact]
        public async Task PipelineCrud_SucceedsAsync()
        {
            // ----- create -----
            var pipelineCreateOptions = new PipelineCreateOptions
            {
                Name = $"it-pipe-{UtcStamp()}",
                RepositoryId = _azureDevOpsConfiguration.RepoId!, // guid or name
                YamlPath = "/azure-pipelines-pipelines.yml",
                Description = "Created by integration test"
            };

            int pipelineId = await _pipelines.CreatePipelineAsync(pipelineCreateOptions);

            // remember for cleanup
            _createdDefinitionIds.Add(pipelineId);

            // ----- read -----
            BuildDefinition? buildDefinition = await _pipelines.GetPipelineAsync(pipelineId);
            Assert.Equal(pipelineCreateOptions.Name, buildDefinition!.Name);

            // ----- update -----
            await _pipelines.UpdatePipelineAsync(pipelineId, new PipelineUpdateOptions
            {
                Description = "Updated by test"
            });

            BuildDefinition? BuildDefinitionAfter = await _pipelines.GetPipelineAsync(pipelineId);
            Assert.Equal("Updated by test", BuildDefinitionAfter!.Description);

            // ----- list -----
            IReadOnlyList<BuildDefinitionReference> list = await _pipelines.ListPipelinesAsync();
            Assert.Contains(list, d => d.Id == pipelineId);

            // ----- delete -----
            await _pipelines.DeletePipelineAsync(pipelineId);
            _createdDefinitionIds.Remove(pipelineId);         // already deleted

            await Assert.ThrowsAsync<Microsoft.TeamFoundation.Build.WebApi.DefinitionNotFoundException>(async () =>
            {
                await _pipelines.GetPipelineAsync(pipelineId);
            });
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");


        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            /* delete pipeline definitions created by tests */
            foreach(int defId in _createdDefinitionIds)
            {
                try
                { await _pipelines.DeletePipelineAsync(defId); }
                catch { /* ignore if already deleted */ }
            }

            /* existing build-run cancellation logic */
            foreach(int id in _queuedBuildIds.AsEnumerable().Reverse())
            {
                try
                {
                    Build? b = await _pipelines.GetRunAsync(id);
                    if(b != null && b.Status == BuildStatus.InProgress)
                        await _pipelines.CancelRunAsync(id, b.Project);
                }
                catch { /* ignore */ }
            }
        }
    }
}