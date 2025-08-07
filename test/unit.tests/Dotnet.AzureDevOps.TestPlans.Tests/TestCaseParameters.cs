namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Helper class for testing test case creation parameter logic
/// </summary>
public class TestCaseParameters
{
    public string? Project { get; set; }
    public string? Title { get; set; }
    public string? Steps { get; set; }
    public int? Priority { get; set; }
    public string? AreaPath { get; set; }
    public string? IterationPath { get; set; }
}
