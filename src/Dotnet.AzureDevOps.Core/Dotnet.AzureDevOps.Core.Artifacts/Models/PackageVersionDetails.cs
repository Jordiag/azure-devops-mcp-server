namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record PackageVersionDetails
{
    public bool Listed { get; init; }
    public IReadOnlyList<string>? Views { get; init; }
}

