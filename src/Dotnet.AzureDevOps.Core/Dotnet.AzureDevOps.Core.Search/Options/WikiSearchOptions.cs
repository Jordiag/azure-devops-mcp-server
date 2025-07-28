namespace Dotnet.AzureDevOps.Core.Search.Options;

public record WikiSearchOptions
{
    public required string SearchText { get; init; }
    public IReadOnlyList<string>? Project { get; init; }
    public IReadOnlyList<string>? Wiki { get; init; }
    public bool IncludeFacets { get; init; }
    public int Skip { get; init; }
    public int Top { get; init; }
}
