﻿using System.IO.Compression;
using System.Threading;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines
{
    public class PipelinesClient : IPipelinesClient
    {
        private readonly string _projectName;
        private readonly BuildHttpClient _build;

        public PipelinesClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _projectName = projectName;

            var connection = new VssConnection(new Uri(organizationUrl),
                          new VssBasicCredential(string.Empty, personalAccessToken));
            _build = connection.GetClient<BuildHttpClient>();
        }

        public async Task<int> QueueRunAsync(BuildQueueOptions buildQueueOptions, CancellationToken cancellationToken = default)
        {
            var build = new Build
            {
                Definition = new DefinitionReference { Id = buildQueueOptions.DefinitionId },
                SourceBranch = buildQueueOptions.Branch,
            };

            if(!string.IsNullOrWhiteSpace(buildQueueOptions.CommitSha))
                build.SourceVersion = buildQueueOptions.CommitSha;

            if(buildQueueOptions.Parameters is { Count: > 0 })
                build.Parameters = System.Text.Json.JsonSerializer.Serialize(buildQueueOptions.Parameters);

            Build queued = await _build.QueueBuildAsync(
                build: build,
                project: _projectName,
                cancellationToken: cancellationToken);
            return queued.Id;
        }

        public Task<Build?> GetRunAsync(int buildId, CancellationToken cancellationToken = default) =>
            _build.GetBuildAsync(
                project: _projectName,
                buildId: buildId,
                cancellationToken: cancellationToken);

        public async Task<IReadOnlyList<Build>> ListRunsAsync(BuildListOptions buildListOptions, CancellationToken cancellationToken = default)
        {
            List<Build> builds = await _build.GetBuildsAsync(
                project: _projectName,
                definitions: buildListOptions.DefinitionId is int d ? new[] { d } : null,
                branchName: buildListOptions.Branch,
                statusFilter: buildListOptions.Status,
                resultFilter: buildListOptions.Result,
                top: buildListOptions.Top,
                cancellationToken: cancellationToken);

            return builds;
        }

        public async Task CancelRunAsync(int buildId, TeamProjectReference project, CancellationToken cancellationToken = default)
        {
            // Fetch the existing build using its numeric ID
            Build build = await _build.GetBuildAsync(
                project: project.Id,
                buildId: buildId,
                cancellationToken: cancellationToken);

            // Set the build status to Cancelling
            build.Status = BuildStatus.Cancelling;

            // Update the build
            await _build.UpdateBuildAsync(
                build: build,
                cancellationToken: cancellationToken);
        }

        public async Task<int> RetryRunAsync(int buildId, CancellationToken cancellationToken = default)
        {
            // 1) fetch the source build
            Build original = await _build.GetBuildAsync(
                project: _projectName,
                buildId: buildId,
                cancellationToken: cancellationToken);

            // 2) re-queue with identical definition / branch / commit / parameters
            var clone = new Build
            {
                Definition = original.Definition,
                SourceBranch = original.SourceBranch,
                SourceVersion = original.SourceVersion,
                Parameters = original.Parameters          // already JSON
            };

            Build queued = await _build.QueueBuildAsync(
                build: clone,
                project: _projectName,
                cancellationToken: cancellationToken);
            return queued.Id;
        }


        public async Task<string?> DownloadConsoleLogAsync(int buildId)
        {
            try
            {
                using Stream buildLogsStream = await _build.GetBuildLogsZipAsync(
                    project: _projectName,
                    buildId: buildId);
                using var zipArchive = new ZipArchive(buildLogsStream);
                ZipArchiveEntry? logEntry = zipArchive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".log"));
                if(logEntry == null)
                    return null;
                using var logReader = new StreamReader(logEntry.Open());
                return await logReader.ReadToEndAsync();
            }
            catch
            {
                return null;        // build not found or logs not ready
            }
        }

        #region PIPELINE DEFINITION CRUD  (Pipelines ▸ Pipelines)

        public async Task<int> CreatePipelineAsync(PipelineCreateOptions pipelineCreateOptions, CancellationToken cancellationToken = default)
        {
            // Repository reference
            var repository = new BuildRepository
            {
                Id = pipelineCreateOptions.RepositoryId,
                Type = "TfsGit",
                DefaultBranch = pipelineCreateOptions.DefaultBranch
            };

            // YAML process
            var yamlProcess = new YamlProcess { YamlFilename = pipelineCreateOptions.YamlPath };

            var definition = new BuildDefinition
            {
                Name = pipelineCreateOptions.Name,
                Repository = repository,
                Process = yamlProcess,
                Path = "\\",              // root folder in “classic” UI
                Description = pipelineCreateOptions.Description,
                QueueStatus = DefinitionQueueStatus.Enabled
            };

            BuildDefinition created = await _build.CreateDefinitionAsync(
                definition: definition,
                project: _projectName,
                cancellationToken: cancellationToken);
            return created.Id;
        }

        public Task<BuildDefinition?> GetPipelineAsync(int definitionId, CancellationToken cancellationToken = default) =>
            _build.GetDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

        public Task<IReadOnlyList<BuildDefinitionReference>> ListPipelinesAsync(CancellationToken cancellationToken = default) =>
            _build.GetDefinitionsAsync(
                project: _projectName,
                cancellationToken: cancellationToken)
                .ContinueWith(task => (IReadOnlyList<BuildDefinitionReference>)task.Result);

        public async Task UpdatePipelineAsync(int definitionId, PipelineUpdateOptions pipelineUpdateOptions, CancellationToken cancellationToken = default)
        {
            BuildDefinition buildDefinition = await _build.GetDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

            if(pipelineUpdateOptions.Name is { Length: > 0 })
                buildDefinition.Name = pipelineUpdateOptions.Name;
            if(pipelineUpdateOptions.Description is { Length: > 0 })
                buildDefinition.Description = pipelineUpdateOptions.Description;
            if(pipelineUpdateOptions.DefaultBranch is { Length: > 0 })
                buildDefinition.Repository.DefaultBranch = pipelineUpdateOptions.DefaultBranch;
            if(pipelineUpdateOptions.YamlPath is { Length: > 0 } && buildDefinition.Process is YamlProcess yp)
                yp.YamlFilename = pipelineUpdateOptions.YamlPath;

            await _build.UpdateDefinitionAsync(
                definition: buildDefinition,
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);
        }

        public Task DeletePipelineAsync(int definitionId, CancellationToken cancellationToken = default) =>
            _build.DeleteDefinitionAsync(
                project: _projectName,
                definitionId: definitionId,
                cancellationToken: cancellationToken);

        #endregion
    }
}
