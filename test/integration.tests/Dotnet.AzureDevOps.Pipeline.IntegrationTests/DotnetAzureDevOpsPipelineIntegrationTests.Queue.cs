using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Dotnet.AzureDevOps.Pipeline.IntegrationTests;

public partial class DotnetAzureDevOpsPipelineIntegrationTests
{
    [Fact]
    public async Task QueueAndCancelBuild_SucceedsAsync()
    {
        var buildQueueOptions = new BuildQueueOptions
        {
            DefinitionId = _definitionId,
            Branch = _branch,
            CommitSha = _commitSha
        };

        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(buildQueueOptions);
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        RegisterCreatedBuild(buildId);

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
    public async Task QueueAndRetryBuild_SucceedsAsync()
    {
        AzureDevOpsActionResult<int> queuedResult = await _pipelines.QueueRunAsync(new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
        Assert.True(queuedResult.IsSuccessful);
        int queuedBuild = queuedResult.Value;
        RegisterCreatedBuild(queuedBuild);

        AzureDevOpsActionResult<int> retryResult = await _pipelines.RetryRunAsync(queuedBuild);
        Assert.True(retryResult.IsSuccessful);
        int retryQueuedBuild = retryResult.Value;
        RegisterCreatedBuild(retryQueuedBuild);
        Assert.NotEqual(queuedBuild, retryQueuedBuild);

        AzureDevOpsActionResult<Build> retriedResult = await _pipelines.GetRunAsync(retryQueuedBuild);
        Assert.True(retriedResult.IsSuccessful);
        Build retried = retriedResult.Value!;
        Assert.Equal(_branch, retried.SourceBranch);
    }

    [Fact]
    public async Task QueueWithParameters_EchoesParameterValues_SucceedsAsync()
    {
        AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>> pipelines = await _pipelines.ListDefinitionsAsync(new BuildDefinitionListOptions
        {
            Name = Constants.PipelineWithParametersName
        });

        BuildDefinitionReference pipeline = pipelines.Value.First();
        var buildQueueOptions = new BuildQueueOptions
        {
            DefinitionId = pipeline.Id,
            Parameters = new Dictionary<string, string>
            {
                { "param1", "coming-from" }, { "param2", "integration-test" }
            }
        };

        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(buildQueueOptions);
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        RegisterCreatedBuild(buildId);

        Build? queuedBuildIdCompleted = await WaitForBuildToCompleteAsync(buildId, 600, 1000, BuildResult.Succeeded);
        Assert.NotNull(queuedBuildIdCompleted);
        Assert.True(queuedBuildIdCompleted!.Result!.Value == BuildResult.Succeeded, "Build did not complete in time.");


        AzureDevOpsActionResult<string> consoleLogResult = await _pipelines.DownloadConsoleLogAsync(buildId, ["Print pipeline parameters"]);
        Assert.NotNull(consoleLogResult);
        Assert.True(consoleLogResult.IsSuccessful);

        string? consoleLog = consoleLogResult.Value;
        Assert.True(buildQueueOptions.Parameters.Values.All(parameter => consoleLog.Contains(parameter)));
    }
}