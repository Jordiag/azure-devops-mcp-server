using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record Feed
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
