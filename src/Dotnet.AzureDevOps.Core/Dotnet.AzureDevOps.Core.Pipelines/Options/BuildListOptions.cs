using Microsoft.TeamFoundation.Build.WebApi;

namespace Dotnet.AzureDevOps.Core.Pipelines.Options;

public record BuildListOptions
{
    public int? DefinitionId { get; init; }

    public string? Branch { get; init; }

    public BuildStatus? Status { get; init; }       // InProgress, Completed, etc.

    public BuildResult? Result { get; init; }       // Succeeded, Failed…

    public int? Top { get; init; } = 50; // default limit
}
