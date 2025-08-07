using System.ComponentModel;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using ModelContextProtocol.Server;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Test Plans operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class TestPlansTools(ITestPlansClient testPlansClient, ILogger<TestPlansTools> logger)
{
    private readonly ITestPlansClient _testPlansClient = testPlansClient;
    private readonly ILogger<TestPlansTools> _logger = logger;

    [McpServerTool, Description("Creates a new test plan in Azure DevOps Test Plans for organizing and managing test cases. Test plans define the overall testing strategy for a release or iteration, including scope, approach, and test environments. Returns the test plan ID.")]
    public async Task<int> CreateTestPlanAsync(TestPlanCreateOptions options) =>
        (await _testPlansClient.CreateTestPlanAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific test plan including name, description, area path, iteration, build information, and associated test suites. The test plan must exist and be accessible.")]
    public async Task<TestPlan> GetTestPlanAsync(int testPlanId) =>
        (await _testPlansClient.GetTestPlanAsync(testPlanId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all test plans in the Azure DevOps project that the current user has access to. Returns basic information about each test plan including name, ID, state, and creation details. Useful for discovering available test plans.")]
    public async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync() =>
        (await _testPlansClient.ListTestPlansAsync()).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a test plan and all its associated test suites and test case associations from Azure DevOps. This does not delete the underlying test case work items, only their association with this plan. Returns true if deletion was successful.")]
    public async Task<bool> DeleteTestPlanAsync(int testPlanId) =>
        (await _testPlansClient.DeleteTestPlanAsync(testPlanId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a test suite.")]
    public async Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions options) =>
        (await _testPlansClient.CreateTestSuiteAsync(testPlanId, options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the root test suite for a plan.")]
    public async Task<TestSuite> GetRootSuiteAsync(int planId) =>
        (await _testPlansClient.GetRootSuiteAsync(planId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists test suites for a test plan.")]
    public async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId) =>
        (await _testPlansClient.ListTestSuitesAsync(testPlanId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a test case.")]
    public async Task<WorkItem> CreateTestCaseAsync(TestCaseCreateOptions options) =>
        (await _testPlansClient.CreateTestCaseAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists test cases in a suite.")]
    public async Task<object> ListTestCasesAsync(int testPlanId, int testSuiteId) =>
        (await _testPlansClient.ListTestCasesAsync(testPlanId, testSuiteId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Adds test cases to a test suite.")]
    public async Task<bool> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds) =>
        (await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves test results and coverage information for a specific build, including passed/failed test counts, test execution details, coverage metrics, and result trends. Useful for build quality assessment and continuous integration reporting.")]
    public async Task<TestResultsDetails> GetTestResultsForBuildAsync(string projectName, int buildId) =>
        (await _testPlansClient.GetTestResultsForBuildAsync(projectName, buildId)).EnsureSuccess(_logger);
}
