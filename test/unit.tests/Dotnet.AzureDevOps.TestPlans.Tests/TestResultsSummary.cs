namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Helper class for testing test results processing logic
/// </summary>
public class TestResultsSummary
{
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public double PassPercentage { get; set; }
    public bool HasResults { get; set; }
}
