using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Dotnet.AzureDevOps.Pipeline.IntegrationTests;

public partial class DotnetAzureDevOpsPipelineIntegrationTests
{
    [Fact]
    public async Task ListBuilds_Filter_WorksAsync()
    {
        AzureDevOpsActionResult<int> queueResult = await _pipelines.QueueRunAsync(new BuildQueueOptions { DefinitionId = _definitionId, Branch = _branch });
        Assert.True(queueResult.IsSuccessful);
        int buildId = queueResult.Value;
        RegisterCreatedBuild(buildId);

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
    public async Task ListDefinitions_FiltersByIdAsync()
    {
        var options = new BuildDefinitionListOptions
        {
            DefinitionIds = new List<int> { _definitionId }
        };

        AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>> listResult = await _pipelines.ListDefinitionsAsync(options);
        Assert.True(listResult.IsSuccessful);
        IReadOnlyList<BuildDefinitionReference> list = listResult.Value!;
        Assert.Contains(list, d => d.Id == _definitionId);
    }

    [Fact]
    public async Task ListDefinitions_Should_ReturnAsync()
    {
        AzureDevOpsActionResult<IReadOnlyList<BuildDefinitionReference>> result = await _pipelines.ListDefinitionsAsync(options: new BuildDefinitionListOptions
        {
            Name = "Dotnet.McpIntegration*"
        });

        Assert.True(result.IsSuccessful);

        IList<string> definitions = result.Value.Select(d => d.Name).ToList();

        Assert.True(definitions.Count >= 2);
        Assert.Contains(definitions, definition => definition == Constants.PipelineSample);
        Assert.Contains(definitions, definition => definition == Constants.PipelineWithParametersName);
    }
}