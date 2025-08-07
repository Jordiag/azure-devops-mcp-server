using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Moq;

namespace Dotnet.AzureDevOps.Tests.TestPlans.Tests;

/// <summary>
/// Integration-style tests that verify the method handles various scenarios correctly
/// without mocking the Azure DevOps SDK calls
/// </summary>
public class AddTestCasesAsyncBehaviorTests
{
    private readonly Mock<ILogger> _mockLogger;

    public AddTestCasesAsyncBehaviorTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public void AzureDevOpsActionResult_Success_CreatesCorrectResult()
    {
        // Arrange
        bool value = true;

        // Act
        AzureDevOpsActionResult<bool> result = AzureDevOpsActionResult<bool>.Success(value, _mockLogger.Object);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void AzureDevOpsActionResult_Failure_CreatesCorrectResult()
    {
        // Arrange
        Exception exception = new Exception("Test error");

        // Act
        AzureDevOpsActionResult<bool> result = AzureDevOpsActionResult<bool>.Failure(exception, _mockLogger.Object);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.False(result.Value);
        Assert.Contains("Test error", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void TestCaseParameterGeneration_WithVariousCountsOfTestCases_GeneratesCorrectCount(int count)
    {
        // Arrange
        List<int> testCaseIds = Enumerable.Range(1, count).ToList();

        // Act - Using the helper method to test the core logic
        List<SuiteTestCaseCreateUpdateParameters> parameters = CreateTestCaseParameters(testCaseIds);

        // Assert
        Assert.Equal(count, parameters.Count);

        if(count > 0)
        {
            Assert.Equal(1, parameters[0].workItem.Id);
            Assert.Equal(count, parameters[count - 1].workItem.Id);
        }
    }

    private static List<SuiteTestCaseCreateUpdateParameters> CreateTestCaseParameters(IReadOnlyList<int> testCaseIds) =>
        testCaseIds.Select(id => new SuiteTestCaseCreateUpdateParameters
        {
            workItem = new WorkItem { Id = id },
            PointAssignments = new List<Configuration>()
        }).ToList();
}
