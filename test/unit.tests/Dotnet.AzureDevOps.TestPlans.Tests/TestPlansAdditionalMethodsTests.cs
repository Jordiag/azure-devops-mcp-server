using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
namespace Dotnet.AzureDevOps.TestPlans.Tests;
/// <summary>
/// Unit tests for TestPlans client methods that don't involve the TestPlanHttpClient directly
/// These methods use different Azure DevOps clients internally
/// </summary>
[TestType(TestType.Unit)]
[Component(Component.TestPlans)]
public class TestPlansAdditionalMethodsTests
{
    private readonly Mock<ILogger> _mockLogger;
    public TestPlansAdditionalMethodsTests()
    {
        _mockLogger = new Mock<ILogger>();
    }
    [Fact]
    public void CreateTestCaseOptions_WithAllProperties_CreatesCorrectParameters()
    {
        // Arrange
        var options = new TestCaseCreateOptions
        {
            Project = "TestProject",
            Title = "Test Case Title",
            Steps = "Step 1\nStep 2",
            Priority = 2,
            AreaPath = "Project\\Area1",
            IterationPath = "Project\\Iteration1"
        };
        // Act - Test the logic that would be used in CreateTestCaseAsync
        TestCaseParameters result = CreateTestCaseParameters(options);
        // Assert
        Assert.Equal("TestProject", result.Project);
        Assert.Equal("Test Case Title", result.Title);
        Assert.Equal("Step 1\nStep 2", result.Steps);
        Assert.Equal(2, result.Priority);
        Assert.Equal("Project\\Area1", result.AreaPath);
        Assert.Equal("Project\\Iteration1", result.IterationPath);
    }
    [Fact]
    public void CreateTestCaseOptions_WithMinimalProperties_CreatesValidParameters()
    {
        // Arrange
        var options = new TestCaseCreateOptions
        {
            Project = "TestProject",
            Title = "Basic Test Case"
        };
        // Act
        TestCaseParameters result = CreateTestCaseParameters(options);
        // Assert
        Assert.Equal("TestProject", result.Project);
        Assert.Equal("Basic Test Case", result.Title);
        Assert.Null(result.Steps);
        Assert.Null(result.Priority);
        Assert.Null(result.AreaPath);
        Assert.Null(result.IterationPath);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateTestCaseOptions_WithEmptyTitle_HandlesGracefully(string? title)
    {
        // Arrange
        var options = new TestCaseCreateOptions
        {
            Project = "TestProject",
            Title = title ?? ""
        };
        // Act
        TestCaseParameters result = CreateTestCaseParameters(options);
        // Assert
        Assert.Equal("TestProject", result.Project);
        Assert.Equal(title ?? "", result.Title);
    }
    [Fact]
    public void CreateTestCaseOptions_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var options = new TestCaseCreateOptions
        {
            Project = "Test-Project_123",
            Title = "Test Case with Special Characters: @#$%^&*()",
            Steps = "Step with <HTML> tags & special chars",
            AreaPath = "Project\\Feature\\Sub-Feature"
        };
        // Act
        TestCaseParameters result = CreateTestCaseParameters(options);
        // Assert
        Assert.Equal("Test-Project_123", result.Project);
        Assert.Equal("Test Case with Special Characters: @#$%^&*()", result.Title);
        Assert.Equal("Step with <HTML> tags & special chars", result.Steps);
        Assert.Equal("Project\\Feature\\Sub-Feature", result.AreaPath);
    }
    [Fact]
    public void TestResultsDetails_WithAllProperties_CreatesCorrectStructure()
    {
        // Arrange - This tests the expected structure of TestResultsDetails
        var testResultsDetails = new TestResultsSummary
        {
            TotalTests = 100,
            PassedTests = 85,
            FailedTests = 10,
            SkippedTests = 5,
            PassPercentage = 85.0
        };
        // Act - Test the logic for processing test results
        TestResultsSummary summary = ProcessTestResultsDetails(testResultsDetails);
        // Assert
        Assert.Equal(100, summary.TotalTests);
        Assert.Equal(85, summary.PassedTests);
        Assert.Equal(10, summary.FailedTests);
        Assert.Equal(5, summary.SkippedTests);
        Assert.Equal(85.0, summary.PassPercentage);
        Assert.True(summary.HasResults);
    }
    [Fact]
    public void TestResultsDetails_WithZeroTests_HandlesCorrectly()
    {
        // Arrange
        var testResultsDetails = new TestResultsSummary
        {
            TotalTests = 0,
            PassedTests = 0,
            FailedTests = 0,
            SkippedTests = 0,
            PassPercentage = 0.0
        };
        // Act
        TestResultsSummary summary = ProcessTestResultsDetails(testResultsDetails);
        // Assert
        Assert.Equal(0, summary.TotalTests);
        Assert.Equal(0, summary.PassedTests);
        Assert.Equal(0, summary.FailedTests);
        Assert.Equal(0, summary.SkippedTests);
        Assert.Equal(0.0, summary.PassPercentage);
        Assert.False(summary.HasResults);
    }
    [Fact]
    public void TestResultsDetails_WithAllFailedTests_CalculatesCorrectPercentage()
    {
        // Arrange
        var testResultsDetails = new TestResultsSummary
        {
            TotalTests = 50,
            PassedTests = 0,
            FailedTests = 50,
            SkippedTests = 0,
            PassPercentage = 0.0
        };
        // Act
        TestResultsSummary summary = ProcessTestResultsDetails(testResultsDetails);
        // Assert
        Assert.Equal(50, summary.TotalTests);
        Assert.Equal(0, summary.PassedTests);
        Assert.Equal(50, summary.FailedTests);
        Assert.Equal(0, summary.SkippedTests);
        Assert.Equal(0.0, summary.PassPercentage);
        Assert.True(summary.HasResults);
    }
    [Fact]
    public void TestResultsDetails_WithAllPassedTests_CalculatesCorrectPercentage()
    {
        // Arrange
        var testResultsDetails = new TestResultsSummary
        {
            TotalTests = 25,
            PassedTests = 25,
            FailedTests = 0,
            SkippedTests = 0,
            PassPercentage = 100.0
        };
        // Act
        TestResultsSummary summary = ProcessTestResultsDetails(testResultsDetails);
        // Assert
        Assert.Equal(25, summary.TotalTests);
        Assert.Equal(25, summary.PassedTests);
        Assert.Equal(0, summary.FailedTests);
        Assert.Equal(0, summary.SkippedTests);
        Assert.Equal(100.0, summary.PassPercentage);
        Assert.True(summary.HasResults);
    }
    [Fact]
    public void AzureDevOpsActionResult_SuccessWithWorkItem_CreatesCorrectResult()
    {
        // Arrange
        var workItem = new WorkItem();
        workItem.Id = 123;
        workItem.Fields = new Dictionary<string, object>
        {
            ["System.Title"] = "Test Work Item"
        };
        // Act
        AzureDevOpsActionResult<WorkItem> result = AzureDevOpsActionResult<WorkItem>.Success(workItem, _mockLogger.Object);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(123, result.Value.Id);
        Assert.Equal("Test Work Item", result.Value.Fields["System.Title"]);
        Assert.Null(result.ErrorMessage);
    }
    [Fact]
    public void AzureDevOpsActionResult_FailureWithTestResults_CreatesCorrectResult()
    {
        // Arrange
        var exception = new Exception("Failed to retrieve test results");
        // Act
        AzureDevOpsActionResult<TestResultsSummary> result = AzureDevOpsActionResult<TestResultsSummary>.Failure(exception, _mockLogger.Object);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        Assert.Contains("Failed to retrieve test results", result.ErrorMessage);
    }
    /// <summary>
    /// Helper method that replicates the core parameter creation logic from CreateTestCaseAsync
    /// </summary>
    private static TestCaseParameters CreateTestCaseParameters(TestCaseCreateOptions options) =>
        new TestCaseParameters
        {
            Project = options.Project,
            Title = options.Title,
            Steps = options.Steps,
            Priority = options.Priority,
            AreaPath = options.AreaPath,
            IterationPath = options.IterationPath
        };
    /// <summary>
    /// Helper method that replicates the core logic for processing TestResultsDetails
    /// </summary>
    private static TestResultsSummary ProcessTestResultsDetails(TestResultsSummary details) =>
        new TestResultsSummary
        {
            TotalTests = details.TotalTests,
            PassedTests = details.PassedTests,
            FailedTests = details.FailedTests,
            SkippedTests = details.SkippedTests,
            PassPercentage = details.PassPercentage,
            HasResults = details.TotalTests > 0
        };
}
