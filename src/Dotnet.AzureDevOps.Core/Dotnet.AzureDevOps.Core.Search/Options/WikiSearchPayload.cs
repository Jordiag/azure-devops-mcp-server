using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Search.Options;

public class WikiSearchPayload
{
    [JsonPropertyName("searchText")]
    public string SearchText { get; set; } = string.Empty;

    [JsonPropertyName("includeFacets")]
    public bool IncludeFacets { get; set; }

    [JsonPropertyName("$skip")]
    public int Skip { get; set; }

    [JsonPropertyName("$top")]
    public int Top { get; set; }

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, IReadOnlyList<string>>? Filters { get; set; }
}
