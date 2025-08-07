using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Moq;

namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Integration-style tests that mock the ITestPlanClient interface to test the full AddTestCasesAsync method logic
/// </summary>
[TestType(TestType.Unit)]
[Component(Component.TestPlans)]
public class AddTestCasesAsyncIntegrationTests
{
    private readonly Mock<ITestPlanClient> _mockTestPlanClient;
    private readonly Mock<ILogger> _mockLogger;
    private readonly TestPlansClientWithInterface _testPlansClient;
    private readonly string _projectName = "TestProject";

    public AddTestCasesAsyncIntegrationTests()
    {
        _mockTestPlanClient = new Mock<ITestPlanClient>();
        _mockLogger = new Mock<ILogger>();

        _testPlansClient = new TestPlansClientWithInterface(
            _mockTestPlanClient.Object,
            _projectName,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AddTestCasesAsync_WithValidInputs_ReturnsSuccessAndCallsApiCorrectlyAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        List<int> testCaseIds = new List<int> { 1001, 1002, 1003 };

        List<TestCase> mockReturnValue = new List<TestCase>();
        _mockTestPlanClient.Setup(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockReturnValue);

        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(result.Value);

        // Verify the method was called with correct parameters
        _mockTestPlanClient.Verify(client => client.AddTestCasesToSuiteAsync(
                It.Is<List<SuiteTestCaseCreateUpdateParameters>>(list =>
                    list.Count == 3 &&
                    list[0].workItem.Id == 1001 &&
                    list[1].workItem.Id == 1002 &&
                    list[2].workItem.Id == 1003 &&
                    list.All(item => item.PointAssignments != null)),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddTestCasesAsync_WithEmptyTestCaseIds_ReturnsSuccessWithEmptyParametersAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        List<int> testCaseIds = new List<int>();

        List<TestCase> mockReturnValue = new List<TestCase>();
        _mockTestPlanClient.Setup(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockReturnValue);

        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(result.Value);

        // Verify the method was called with empty list
        _mockTestPlanClient.Verify(client => client.AddTestCasesToSuiteAsync(
                It.Is<List<SuiteTestCaseCreateUpdateParameters>>(list => list.Count == 0),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddTestCasesAsync_WhenApiThrowsException_ReturnsFailureWithErrorMessageAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        List<int> testCaseIds = new List<int> { 1001, 1002 };
        Exception expectedException = new Exception("Azure DevOps API error");

        _mockTestPlanClient.Setup(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.False(result.Value);
        Assert.Contains("Azure DevOps API error", result.ErrorMessage);
    }

    [Fact]
    public async Task AddTestCasesAsync_WithCancellationToken_PassesTokenCorrectlyAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        List<int> testCaseIds = new List<int> { 1001 };
        using CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        List<TestCase> mockReturnValue = new List<TestCase>();
        _mockTestPlanClient.Setup(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                cancellationToken))
            .ReturnsAsync(mockReturnValue);

        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds, cancellationToken);

        // Assert
        Assert.True(result.IsSuccessful);

        // Verify the cancellation token was passed through
        _mockTestPlanClient.Verify(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task AddTestCasesAsync_CreatesCorrectSuiteTestCaseParametersAsync()
    {
        // Arrange
        int testPlanId = 123;
        int testSuiteId = 456;
        List<int> testCaseIds = new List<int> { 1001, 1002 };

        List<TestCase> mockReturnValue = new List<TestCase>();
        List<SuiteTestCaseCreateUpdateParameters>? capturedParameters = null;

        _mockTestPlanClient.Setup(client => client.AddTestCasesToSuiteAsync(
                It.IsAny<List<SuiteTestCaseCreateUpdateParameters>>(),
                _projectName,
                testPlanId,
                testSuiteId,
                It.IsAny<CancellationToken>()))
            .Callback<List<SuiteTestCaseCreateUpdateParameters>, string, int, int, CancellationToken>(
                (parameters, project, planId, suiteId, ct) => capturedParameters = parameters)
            .ReturnsAsync(mockReturnValue);

        // Act
        AzureDevOpsActionResult<bool> result = await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(capturedParameters);
        Assert.Equal(2, capturedParameters.Count);

        // Verify first parameter
        Assert.Equal(1001, capturedParameters[0].workItem.Id);
        Assert.NotNull(capturedParameters[0].PointAssignments);
        Assert.Empty(capturedParameters[0].PointAssignments);

        // Verify second parameter
        Assert.Equal(1002, capturedParameters[1].workItem.Id);
        Assert.NotNull(capturedParameters[1].PointAssignments);
        Assert.Empty(capturedParameters[1].PointAssignments);
    }
}
