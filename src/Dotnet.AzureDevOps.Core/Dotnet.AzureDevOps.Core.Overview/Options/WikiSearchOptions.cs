using System.Collections.Generic;

namespace Dotnet.AzureDevOps.Core.Overview.Options;

/// <summary>
/// Options for searching wiki pages.
/// </summary>
public record WikiSearchOptions
{
    public string SearchText { get; init; } = string.Empty;

    public IReadOnlyList<string>? Project { get; init; }

    public IReadOnlyList<string>? Wiki { get; init; }

    public bool IncludeFacets { get; init; }

    public int Skip { get; init; }

    public int Top { get; init; } = 10;
}
