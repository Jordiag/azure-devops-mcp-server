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
    public async Task<int> CreateTestPlanAsync(TestPlanCreateOptions options, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.CreateTestPlanAsync(options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific test plan including name, description, area path, iteration, build information, and associated test suites. The test plan must exist and be accessible.")]
    public async Task<TestPlan> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.GetTestPlanAsync(testPlanId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all test plans in the Azure DevOps project that the current user has access to. Returns basic information about each test plan including name, ID, state, and creation details. Useful for discovering available test plans.")]
    public async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(CancellationToken cancellationToken = default) =>
        (await _testPlansClient.ListTestPlansAsync(cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a test plan and all its associated test suites and test case associations from Azure DevOps. This does not delete the underlying test case work items, only their association with this plan. Returns true if deletion was successful.")]
    public async Task<bool> DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.DeleteTestPlanAsync(testPlanId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new test suite within a test plan for organizing and grouping test cases. Test suites help structure test execution by requirements, features, or test type. Supports static suites (manual test case selection) and query-based suites (dynamic based on work item queries). Returns the unique test suite ID.")]
    public async Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions options, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.CreateTestSuiteAsync(testPlanId, options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the root test suite for a test plan, which is the top-level container that holds all other test suites in the hierarchy. Every test plan has exactly one root suite that serves as the parent for all test case organization. Returns the root test suite with its configuration and child suite information.")]
    public async Task<TestSuite> GetRootSuiteAsync(int planId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.GetRootSuiteAsync(planId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all test suites within a specific test plan, including both static and query-based suites. Returns suite information such as names, types, parent-child relationships, and test case counts. Useful for understanding test organization structure and navigating the test plan hierarchy.")]
    public async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.ListTestSuitesAsync(testPlanId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new test case work item in Azure DevOps with test-specific fields like test steps, expected results, and automation status. Test cases define the specific conditions, actions, and expected outcomes for validating functionality. Returns the created test case as a work item with all fields populated.")]
    public async Task<WorkItem> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.CreateTestCaseAsync(options, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Lists all test cases associated with a specific test suite within a test plan. Returns test case information including work item IDs, titles, states, assigned testers, and test outcomes. This helps identify which test cases are included in a test suite for execution planning and progress tracking.")]
    public async Task<object> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.ListTestCasesAsync(testPlanId, testSuiteId, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Adds existing test case work items to a test suite, creating the association between test cases and the suite for execution. Test cases can be added to multiple suites and will appear in test execution for each suite they belong to. Returns true if all test cases were successfully added to the suite.")]
    public async Task<bool> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds, cancellationToken)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves test results and coverage information for a specific build, including passed/failed test counts, test execution details, coverage metrics, and result trends. Useful for build quality assessment and continuous integration reporting.")]
    public async Task<TestResultsDetails> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default) =>
        (await _testPlansClient.GetTestResultsForBuildAsync(projectName, buildId, cancellationToken)).EnsureSuccess(_logger);
}
