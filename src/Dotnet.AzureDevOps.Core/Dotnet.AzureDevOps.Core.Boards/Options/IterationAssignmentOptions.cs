namespace Dotnet.AzureDevOps.Core.Boards.Options;

public record IterationAssignmentOptions
{
    public required string Identifier { get; init; }
    public required string Path { get; init; }
}
