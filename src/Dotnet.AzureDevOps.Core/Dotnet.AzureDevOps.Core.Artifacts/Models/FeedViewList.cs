using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedViewList([property: JsonPropertyName("value")] List<FeedView> Value);

