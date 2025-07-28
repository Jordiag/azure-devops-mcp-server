using Microsoft.TeamFoundation.Build.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines.Options;

public record BuildDefinitionListOptions
{
    public string? RepositoryId { get; init; }
    public string? RepositoryType { get; init; }
    public string? Name { get; init; }
    public string? Path { get; init; }
    public DefinitionQueryOrder? QueryOrder { get; init; }
    public int? Top { get; init; }
    public string? ContinuationToken { get; init; }
    public DateTime? MinMetricsTimeInUtc { get; init; }
    public IReadOnlyList<int>? DefinitionIds { get; init; }
    public DateTime? BuiltAfter { get; init; }
    public DateTime? NotBuiltAfter { get; init; }
    public bool? IncludeAllProperties { get; init; }
    public bool? IncludeLatestBuilds { get; init; }
    public Guid? TaskIdFilter { get; init; }
    public int? ProcessType { get; init; }
    public string? YamlFilename { get; init; }
}
