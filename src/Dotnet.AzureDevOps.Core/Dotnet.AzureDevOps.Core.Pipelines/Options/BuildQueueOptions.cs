namespace Dotnet.AzureDevOps.Core.Pipelines.Options;

public record BuildQueueOptions
{
    public int DefinitionId { get; init; }      // pipeline ID (classic or YAML)

    public string? Branch { get; init; }      // refs/heads/main etc.

    public string? CommitSha { get; init; }      // optional specific commit

    public Dictionary<string, string>? Parameters { get; init; }   // runtime vars
}
