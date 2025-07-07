using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record Package
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
