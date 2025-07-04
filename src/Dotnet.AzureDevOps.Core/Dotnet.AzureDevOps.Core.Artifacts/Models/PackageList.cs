using System.Text.Json.Serialization;

namespace Dotnet.AzureDevOps.Core.Artifacts.Models
{
    public record PackageList([property: JsonPropertyName("value")] List<Package> Value);
}