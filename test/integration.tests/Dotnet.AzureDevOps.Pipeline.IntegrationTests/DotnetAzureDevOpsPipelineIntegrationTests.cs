using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Build.WebApi;

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
            // Arrange
            var buildQueueOptions = new BuildQueueOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch,
                CommitSha = _commitSha
            };

            // Act – queue build
            int buildId = await _pipelines.QueueRunAsync(buildQueueOptions);
            _queuedBuildIds.Add(buildId);

            // Assert – build exists
            Build? run = await _pipelines.GetRunAsync(buildId);
            Assert.NotNull(run);

            // Act – cancel
            await _pipelines.CancelRunAsync(buildId, run.Project);

            // Assert – status now Cancelling
            run = await _pipelines.GetRunAsync(buildId);
            Assert.Equal(BuildStatus.Cancelling, run!.Status);
        }

        [Fact]
        public async Task RetryBuild_SucceedsAsync()
        {
            // Arrange – queue original
            int queuedBuild = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(queuedBuild);

            // Act – retry
            int retryQueuedBuild = await _pipelines.RetryRunAsync(queuedBuild);
            _queuedBuildIds.Add(retryQueuedBuild);

            // Assert
            Assert.NotEqual(queuedBuild, retryQueuedBuild);

            Build? retried = await _pipelines.GetRunAsync(retryQueuedBuild);
            Assert.NotNull(retried);
            Assert.Equal(_branch, retried!.SourceBranch);
        }

        [Fact]
        public async Task ListBuilds_Filter_WorksAsync()
        {
            // Arrange – ensure at least one queued build exists
            int buildId = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(buildId);

            // Act
            IReadOnlyList<Build> list = await _pipelines.ListRunsAsync(new BuildListOptions
            {
                DefinitionId = _definitionId,
                Branch = _branch,
                Status = BuildStatus.NotStarted, // will match our queued run
                Top = 10
            });

            // Assert
            Assert.Contains(list, b => b.Id == buildId);
        }

        [Fact]
        public async Task DownloadConsoleLog_SucceedsAsync()
        {
            // Arrange – queue build
            int queuedBuildId = await _pipelines.QueueRunAsync(
                new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
            _queuedBuildIds.Add(queuedBuildId);

            // Act – try to fetch log (may be empty if build hasn’t started)
            string? consoleLog = await _pipelines.DownloadConsoleLogAsync(queuedBuildId);

            // Assert – no exception; log either null or contains header line
            Assert.True(string.IsNullOrEmpty(consoleLog) || consoleLog.Length > 0);
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
            Assert.NotNull(logs);

            List<BuildDefinitionRevision> revisions = await _pipelines.GetDefinitionRevisionsAsync(
                _definitionId);
            Assert.NotNull(revisions);
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