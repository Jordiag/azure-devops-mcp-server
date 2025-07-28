namespace Dotnet.AzureDevOps.Core.TestPlans.Options;

public record TestCaseCreateOptions
{
    public required string Title { get; init; }
    public string? Steps { get; init; }
    public int? Priority { get; init; }
    public string? AreaPath { get; init; }
    public string? IterationPath { get; init; }
    public required string Project { get; init; }
}
