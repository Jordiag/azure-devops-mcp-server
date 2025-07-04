using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview.Options;

/// <summary>
/// Parameters required to create a new wiki.
/// </summary>
public record WikiCreateOptions
{
    public string Name { get; init; } = string.Empty;

    public Guid ProjectId { get; init; }

    public Guid RepositoryId { get; init; }

    public string? MappedPath { get; init; }

    public GitVersionDescriptor? Version { get; init; }

    public WikiType Type { get; init; }
}
