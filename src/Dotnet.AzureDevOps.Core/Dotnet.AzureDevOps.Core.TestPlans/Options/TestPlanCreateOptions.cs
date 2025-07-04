namespace Dotnet.AzureDevOps.Core.TestPlans.Options;

public record TestPlanCreateOptions
{
    public string Name { get; init; } = string.Empty;
    public string? AreaPath { get; init; }
    public string? Iteration { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Description { get; init; }
}
