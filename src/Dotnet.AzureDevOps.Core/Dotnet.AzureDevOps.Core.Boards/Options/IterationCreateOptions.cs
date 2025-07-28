namespace Dotnet.AzureDevOps.Core.Boards.Options;

public record IterationCreateOptions
{
    public required string IterationName { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? FinishDate { get; init; }
}
