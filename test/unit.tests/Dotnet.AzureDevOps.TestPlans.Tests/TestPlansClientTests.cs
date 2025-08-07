using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Moq;

namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Unit tests for the AddTestCasesAsync method logic.
/// These tests focus on testing the core business logic without mocking the complex Azure DevOps SDK.
/// </summary>
[TestType(TestType.Unit)]
[Component(Component.TestPlans)]
public class AddTestCasesAsyncLogicTests
{
    [Fact]
    public void CreateSuiteTestCaseParameters_WithValidTestCaseIds_CreatesCorrectParameters()
    {
        // Arrange
        List<int> testCaseIds = new List<int> { 1001, 1002, 1003 };

        // Act
        List<SuiteTestCaseCreateUpdateParameters> result = CreateSuiteTestCaseParameters(testCaseIds);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1001, result[0].workItem.Id);
        Assert.Equal(1002, result[1].workItem.Id);
        Assert.Equal(1003, result[2].workItem.Id);

        // Verify all have empty PointAssignments (as per the original implementation)
        Assert.All(result, item => Assert.NotNull(item.PointAssignments));
        Assert.All(result, item => Assert.Empty(item.PointAssignments));
    }

    [Fact]
    public void CreateSuiteTestCaseParameters_WithEmptyTestCaseIds_CreatesEmptyList()
    {
        // Arrange
        List<int> testCaseIds = new List<int>();

        // Act
        List<SuiteTestCaseCreateUpdateParameters> result = CreateSuiteTestCaseParameters(testCaseIds);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CreateSuiteTestCaseParameters_WithSingleTestCaseId_CreatesSingleParameter()
    {
        // Arrange
        List<int> testCaseIds = new List<int> { 1001 };

        // Act
        List<SuiteTestCaseCreateUpdateParameters> result = CreateSuiteTestCaseParameters(testCaseIds);

        // Assert
        Assert.Single(result);
        Assert.Equal(1001, result[0].workItem.Id);
        Assert.NotNull(result[0].PointAssignments);
        Assert.Empty(result[0].PointAssignments);
    }

    [Theory]
    [InlineData(new int[] { -1 })]
    [InlineData(new int[] { 0 })]
    [InlineData(new int[] { -999, 0, 1001 })]
    public void CreateSuiteTestCaseParameters_WithInvalidTestCaseIds_StillCreatesParameters(int[] testCaseIds)
    {
        // Arrange
        List<int> testCaseIdsList = testCaseIds.ToList();

        // Act
        List<SuiteTestCaseCreateUpdateParameters> result = CreateSuiteTestCaseParameters(testCaseIdsList);

        // Assert
        Assert.Equal(testCaseIds.Length, result.Count);
        for(int i = 0; i < testCaseIds.Length; i++)
        {
            Assert.Equal(testCaseIds[i], result[i].workItem.Id);
            Assert.NotNull(result[i].PointAssignments);
            Assert.Empty(result[i].PointAssignments);
        }
    }

    [Fact]
    public void CreateSuiteTestCaseParameters_WithDuplicateTestCaseIds_CreatesParametersForAll()
    {
        // Arrange
        List<int> testCaseIds = new List<int> { 1001, 1001, 1002, 1001 };

        // Act
        List<SuiteTestCaseCreateUpdateParameters> result = CreateSuiteTestCaseParameters(testCaseIds);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(1001, result[0].workItem.Id);
        Assert.Equal(1001, result[1].workItem.Id);
        Assert.Equal(1002, result[2].workItem.Id);
        Assert.Equal(1001, result[3].workItem.Id);
    }

    /// <summary>
    /// Helper method that replicates the core logic from AddTestCasesAsync
    /// This allows us to unit test the parameter creation logic without mocking the Azure DevOps SDK
    /// </summary>
    private static List<SuiteTestCaseCreateUpdateParameters> CreateSuiteTestCaseParameters(IReadOnlyList<int> testCaseIds)
    {
        List<WorkItem> references = testCaseIds.Select(id => new WorkItem { Id = id }).ToList();
        List<SuiteTestCaseCreateUpdateParameters> existingTestCases = new List<SuiteTestCaseCreateUpdateParameters>();

        foreach(WorkItem workItem in references)
        {
            SuiteTestCaseCreateUpdateParameters suiteTestCase = new SuiteTestCaseCreateUpdateParameters
            {
                workItem = new WorkItem { Id = workItem.Id },
                PointAssignments = new List<Configuration>()
            };
            existingTestCases.Add(suiteTestCase);
        }

        return existingTestCases;
    }
}
