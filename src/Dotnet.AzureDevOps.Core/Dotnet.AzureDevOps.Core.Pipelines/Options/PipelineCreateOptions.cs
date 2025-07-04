namespace Dotnet.AzureDevOps.Core.Pipelines.Options;

/// <summary>Minimal data to create a new YAML pipeline definition.</summary>
public record PipelineCreateOptions
{
    public string Name { get; init; } = "new-pipeline";

    public string RepositoryId { get; init; } = string.Empty;          // GUID or repo name

    public string YamlPath { get; init; } = "/azure-pipelines.yml"; // repo-relative path

    public string DefaultBranch { get; init; } = "refs/heads/main";

    public string? Description { get; init; }
}
