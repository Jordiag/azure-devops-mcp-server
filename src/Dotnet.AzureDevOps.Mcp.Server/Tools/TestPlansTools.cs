using System.ComponentModel;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Test Plans operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class TestPlansTools
{
    private readonly ITestPlansClient _testPlansClient;
    private readonly ILogger<TestPlansTools> _logger;

    public TestPlansTools(ITestPlansClient testPlansClient, ILogger<TestPlansTools> logger)
    {
        _testPlansClient = testPlansClient;
        _logger = logger;
    }

    [McpServerTool, Description("Creates a test plan.")]
    public async Task<int> CreateTestPlanAsync(TestPlanCreateOptions options)
    {
        return (await _testPlansClient.CreateTestPlanAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a test plan.")]
    public async Task<TestPlan> GetTestPlanAsync(int testPlanId)
    {
        return (await _testPlansClient.GetTestPlanAsync(testPlanId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists test plans.")]
    public async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync()
    {
        return (await _testPlansClient.ListTestPlansAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a test plan.")]
    public async Task<bool> DeleteTestPlanAsync(int testPlanId)
    {
        return (await _testPlansClient.DeleteTestPlanAsync(testPlanId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a test suite.")]
    public async Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions options)
    {
        return (await _testPlansClient.CreateTestSuiteAsync(testPlanId, options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves the root test suite for a plan.")]
    public async Task<TestSuite> GetRootSuiteAsync(int planId)
    {
        return (await _testPlansClient.GetRootSuiteAsync(planId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists test suites for a test plan.")]
    public async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId)
    {
        return (await _testPlansClient.ListTestSuitesAsync(testPlanId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a test case.")]
    public async Task<WorkItem> CreateTestCaseAsync(TestCaseCreateOptions options)
    {
        return (await _testPlansClient.CreateTestCaseAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists test cases in a suite.")]
    public async Task<object> ListTestCasesAsync(int testPlanId, int testSuiteId)
    {
        return (await _testPlansClient.ListTestCasesAsync(testPlanId, testSuiteId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Adds test cases to a test suite.")]
    public async Task<bool> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds)
    {
        return (await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets test results for a build.")]
    public async Task<TestResultsDetails> GetTestResultsForBuildAsync(string projectName, int buildId)
    {
        return (await _testPlansClient.GetTestResultsForBuildAsync(projectName, buildId)).EnsureSuccess(_logger);
    }
}
