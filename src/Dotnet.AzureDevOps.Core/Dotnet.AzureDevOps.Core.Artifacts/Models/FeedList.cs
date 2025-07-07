using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedList([property: JsonPropertyName("value")] List<Feed> Value);