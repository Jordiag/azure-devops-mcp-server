using System.ComponentModel;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Test Plans operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class TestPlansTools
{
    private static TestPlansClient CreateClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    [McpServerTool, Description("Creates a test plan.")]
    public static async Task<int> CreateTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, TestPlanCreateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateTestPlanAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a test plan.")]
    public static async Task<TestPlan> GetTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetTestPlanAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test plans.")]
    public static async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListTestPlansAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a test plan.")]
    public static async Task<bool> DeleteTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteTestPlanAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a test suite.")]
    public static async Task<int> CreateTestSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, TestSuiteCreateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateTestSuiteAsync(testPlanId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test suites for a plan.")]
    public static async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListTestSuitesAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Adds test cases to a suite.")]
    public static async Task<bool> AddTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets the root suite of a test plan.")]
    public static async Task<TestSuite> GetRootSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int planId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetRootSuiteAsync(planId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test cases in a suite.")]
    public static async Task<PagedList<TestCase>> ListTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListTestCasesAsync(testPlanId, testSuiteId)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets test results for a build.")]
    public static async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestResultsDetails> GetTestResultsForBuildAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetTestResultsForBuildAsync(projectName, buildId)).EnsureSuccess();
    }
}