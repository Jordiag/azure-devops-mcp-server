namespace Dotnet.AzureDevOps.Core.Search.Options;

public record CodeSearchOptions
{
    public required string SearchText { get; init; }
    public IReadOnlyList<string>? Project { get; init; }
    public IReadOnlyList<string>? Repository { get; init; }
    public IReadOnlyList<string>? Path { get; init; }
    public IReadOnlyList<string>? Branch { get; init; }
    public bool IncludeFacets { get; init; }
    public int Skip { get; init; }
    public int Top { get; init; }
}
