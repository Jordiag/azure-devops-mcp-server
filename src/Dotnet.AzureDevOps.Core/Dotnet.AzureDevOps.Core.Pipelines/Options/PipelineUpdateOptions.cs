namespace Dotnet.AzureDevOps.Core.Pipelines.Options;

/// <summary>Fields you may update after creation.</summary>
public record PipelineUpdateOptions
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? DefaultBranch { get; init; }

    public string? YamlPath { get; init; }
}
