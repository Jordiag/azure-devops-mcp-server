using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedPermissionList([property: JsonPropertyName("value")] List<FeedPermission> Value);

