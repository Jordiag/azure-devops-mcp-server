using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Moq;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
namespace Dotnet.AzureDevOps.TestPlans.Tests;
/// <summary>
/// Unit tests for TestPlans client methods
/// </summary>
[TestType(TestType.Unit)]
[Component(Component.TestPlans)]
public class TestPlansClientUnitTests
{
    private readonly Mock<ITestPlanClient> _mockTestPlanClient;
    private readonly Mock<ILogger> _mockLogger;
    private readonly TestPlansClientWithMockedInterface _testPlansClient;
    private readonly string _projectName = "TestProject";
    public TestPlansClientUnitTests()
    {
        _mockTestPlanClient = new Mock<ITestPlanClient>();
        _mockLogger = new Mock<ILogger>();
        _testPlansClient = new TestPlansClientWithMockedInterface(_mockTestPlanClient.Object, _projectName, _mockLogger.Object);
    }
    [Fact]
    public async Task CreateTestPlanAsync_WithValidOptions_ReturnsSuccessWithTestPlanIdAsync()
    {
        // Arrange
        var options = new TestPlanCreateOptions
        {
            Name = "Test Plan 1",
            Description = "Test Description",
            AreaPath = "Area1",
            Iteration = "Iteration1"
        };
        var mockTestPlan = new TestPlan { Id = 123, Name = "Test Plan 1" };
        _mockTestPlanClient
            .Setup(client => client.CreateTestPlanAsync(It.IsAny<TestPlanCreateParams>(), _projectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestPlan);
        // Act
        AzureDevOpsActionResult<int> result = await _testPlansClient.CreateTestPlanAsync(options);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(123, result.Value);
        _mockTestPlanClient.Verify(
            client => client.CreateTestPlanAsync(
                It.Is<TestPlanCreateParams>(p => p.Name == "Test Plan 1" && p.Description == "Test Description"),
                _projectName,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task CreateTestPlanAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        var options = new TestPlanCreateOptions { Name = "Test Plan 1" };
        var exception = new Exception("API error");
        _mockTestPlanClient
            .Setup(client => client.CreateTestPlanAsync(It.IsAny<TestPlanCreateParams>(), _projectName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        // Act
        AzureDevOpsActionResult<int> result = await _testPlansClient.CreateTestPlanAsync(options);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("API error", result.ErrorMessage);
    }
    [Fact]
    public async Task GetTestPlanAsync_WithValidId_ReturnsTestPlanAsync()
    {
        // Arrange
        int testPlanId = 123;
        var mockTestPlan = new TestPlan { Id = testPlanId, Name = "Test Plan 1" };
        _mockTestPlanClient
            .Setup(client => client.GetTestPlanByIdAsync(_projectName, testPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestPlan);
        // Act
        AzureDevOpsActionResult<TestPlan> result = await _testPlansClient.GetTestPlanAsync(testPlanId);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(testPlanId, result.Value.Id);
        Assert.Equal("Test Plan 1", result.Value.Name);
    }
    [Fact]
    public async Task GetTestPlanAsync_WithInvalidId_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 999;
        _mockTestPlanClient
            .Setup(client => client.GetTestPlanByIdAsync(_projectName, testPlanId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test plan not found"));
        // Act
        AzureDevOpsActionResult<TestPlan> result = await _testPlansClient.GetTestPlanAsync(testPlanId);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("Test plan not found", result.ErrorMessage);
    }
    [Fact]
    public async Task ListTestPlansAsync_ReturnsListOfTestPlansAsync()
    {
        // Arrange
        var mockTestPlans = new PagedList<TestPlan>(new List<TestPlan>
        {
            new TestPlan { Id = 1, Name = "Plan 1" },
            new TestPlan { Id = 2, Name = "Plan 2" }
        }, null);
        _mockTestPlanClient
            .Setup(client => client.GetTestPlansAsync(_projectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestPlans);
        // Act
        AzureDevOpsActionResult<IReadOnlyList<TestPlan>> result = await _testPlansClient.ListTestPlansAsync();
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Plan 1", result.Value[0].Name);
        Assert.Equal("Plan 2", result.Value[1].Name);
    }
    [Fact]
    public async Task ListTestPlansAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        _mockTestPlanClient
            .Setup(client => client.GetTestPlansAsync(_projectName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));
        // Act
        AzureDevOpsActionResult<IReadOnlyList<TestPlan>> result = await _testPlansClient.ListTestPlansAsync();
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("API error", result.ErrorMessage);
    }
    [Fact]
    public async Task DeleteTestPlanAsync_WithValidId_ReturnsSuccessAsync()
    {
        // Arrange
        int testPlanId = 123;
        _mockTestPlanClient
            .Setup(client => client.DeleteTestPlanAsync(_projectName, testPlanId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.DeleteTestPlanAsync(testPlanId);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(result.Value);
        _mockTestPlanClient.Verify(
            client => client.DeleteTestPlanAsync(_projectName, testPlanId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task DeleteTestPlanAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 123;
        _mockTestPlanClient
            .Setup(client => client.DeleteTestPlanAsync(_projectName, testPlanId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete test plan"));
        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.DeleteTestPlanAsync(testPlanId);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("Cannot delete test plan", result.ErrorMessage);
    }
    [Fact]
    public async Task CreateTestSuiteAsync_WithValidOptions_ReturnsSuccessWithSuiteIdAsync()
    {
        // Arrange
        int testPlanId = 123;
        var options = new TestSuiteCreateOptions
        {
            Name = "Test Suite 1",
            ParentSuite = new TestSuiteReference { Id = 456 }
        };
        var mockTestSuite = new TestSuite { Id = 789, Name = "Test Suite 1" };
        _mockTestPlanClient
            .Setup(client => client.CreateTestSuiteAsync(It.IsAny<TestSuiteCreateParams>(), _projectName, testPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestSuite);
        // Act
        AzureDevOpsActionResult<int> result = await _testPlansClient.CreateTestSuiteAsync(testPlanId, options);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(789, result.Value);
        _mockTestPlanClient.Verify(
            client => client.CreateTestSuiteAsync(
                It.Is<TestSuiteCreateParams>(p => p.Name == "Test Suite 1"),
                _projectName,
                testPlanId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task CreateTestSuiteAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 123;
        var options = new TestSuiteCreateOptions
        {
            Name = "Test Suite 1",
            ParentSuite = new TestSuiteReference { Id = 456 }
        };
        _mockTestPlanClient
            .Setup(client => client.CreateTestSuiteAsync(It.IsAny<TestSuiteCreateParams>(), _projectName, testPlanId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot create test suite"));
        // Act
        AzureDevOpsActionResult<int> result = await _testPlansClient.CreateTestSuiteAsync(testPlanId, options);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("Cannot create test suite", result.ErrorMessage);
    }
    [Fact]
    public async Task ListTestSuitesAsync_WithValidPlanId_ReturnsListOfSuitesAsync()
    {
        // Arrange
        int testPlanId = 123;
        var mockTestSuites = new PagedList<TestSuite>(new List<TestSuite>
        {
            new TestSuite { Id = 1, Name = "Suite 1" },
            new TestSuite { Id = 2, Name = "Suite 2" }
        }, null);
        _mockTestPlanClient
            .Setup(client => client.GetTestSuitesForPlanAsync(_projectName, testPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestSuites);
        // Act
        AzureDevOpsActionResult<IReadOnlyList<TestSuite>> result = await _testPlansClient.ListTestSuitesAsync(testPlanId);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Suite 1", result.Value[0].Name);
        Assert.Equal("Suite 2", result.Value[1].Name);
    }
    [Fact]
    public async Task ListTestSuitesAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 123;
        _mockTestPlanClient
            .Setup(client => client.GetTestSuitesForPlanAsync(_projectName, testPlanId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot list test suites"));
        // Act
        AzureDevOpsActionResult<IReadOnlyList<TestSuite>> result = await _testPlansClient.ListTestSuitesAsync(testPlanId);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("Cannot list test suites", result.ErrorMessage);
    }
    [Fact]
    public async Task ListTestCasesAsync_WithValidIds_ReturnsListOfTestCasesAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        var mockTestCases = new PagedList<TestCase>(new List<TestCase>
        {
            new TestCase { workItem = new WorkItemDetails { Id = 1 } },
            new TestCase { workItem = new WorkItemDetails { Id = 2 } }
        }, null);
        _mockTestPlanClient
            .Setup(client => client.GetTestCaseListAsync(_projectName, testPlanId, testSuiteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestCases);
        // Act
        AzureDevOpsActionResult<PagedList<TestCase>> result = await _testPlansClient.ListTestCasesAsync(testPlanId, testSuiteId);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(1, result.Value[0].workItem.Id);
        Assert.Equal(2, result.Value[1].workItem.Id);
    }
    [Fact]
    public async Task ListTestCasesAsync_WhenApiThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        _mockTestPlanClient
            .Setup(client => client.GetTestCaseListAsync(_projectName, testPlanId, testSuiteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot list test cases"));
        // Act
        AzureDevOpsActionResult<PagedList<TestCase>> result = await _testPlansClient.ListTestCasesAsync(testPlanId, testSuiteId);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains("Cannot list test cases", result.ErrorMessage);
    }
    [Fact]
    public async Task GetRootSuiteAsync_WithValidPlanId_ReturnsRootSuiteAsync()
    {
        // Arrange
        int testPlanId = 123;
        var mockTestSuites = new PagedList<TestSuite>(new List<TestSuite>
        {
            new TestSuite { Id = 1, Name = "Root Suite", ParentSuite = null },
            new TestSuite { Id = 2, Name = "Child Suite", ParentSuite = new TestSuiteReference { Id = 1 } }
        }, null);
        _mockTestPlanClient
            .Setup(client => client.GetTestSuitesForPlanAsync(_projectName, testPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestSuites);
        // Act
        AzureDevOpsActionResult<TestSuite> result = await _testPlansClient.GetRootSuiteAsync(testPlanId);
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Root Suite", result.Value.Name);
        Assert.Null(result.Value.ParentSuite);
    }
    [Fact]
    public async Task GetRootSuiteAsync_WhenListSuitesReturnsEmpty_ReturnsFailureAsync()
    {
        // Arrange
        int testPlanId = 123;
        var mockTestSuites = new PagedList<TestSuite>(new List<TestSuite>(), null);
        _mockTestPlanClient
            .Setup(client => client.GetTestSuitesForPlanAsync(_projectName, testPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTestSuites);
        // Act
        AzureDevOpsActionResult<TestSuite> result = await _testPlansClient.GetRootSuiteAsync(testPlanId);
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Contains($"No root suite found for test plan {testPlanId}", result.ErrorMessage);
    }
}
