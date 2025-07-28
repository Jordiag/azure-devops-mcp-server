namespace Dotnet.AzureDevOps.Core.Search.Options;

public record WorkItemSearchOptions
{
    public required string SearchText { get; init; }
    public IReadOnlyList<string>? Project { get; init; }
    public IReadOnlyList<string>? AreaPath { get; init; }
    public IReadOnlyList<string>? WorkItemType { get; init; }
    public IReadOnlyList<string>? State { get; init; }
    public IReadOnlyList<string>? AssignedTo { get; init; }
    public bool IncludeFacets { get; init; }
    public int Skip { get; init; }
    public int Top { get; init; }
}
