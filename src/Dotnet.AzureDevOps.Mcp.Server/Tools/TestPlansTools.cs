using System.ComponentModel;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Test Plans operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class TestPlansTools
{
    private static TestPlansClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a test plan.")]
    public static async Task<int> CreateTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, TestPlanCreateOptions options)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateTestPlanAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a test plan.")]
    public static async Task<TestPlan> GetTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetTestPlanAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test plans.")]
    public static async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListTestPlansAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a test plan.")]
    public static async Task<bool> DeleteTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteTestPlanAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a test suite.")]
    public static async Task<int> CreateTestSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, TestSuiteCreateOptions options)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateTestSuiteAsync(testPlanId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test suites for a plan.")]
    public static async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListTestSuitesAsync(testPlanId)).EnsureSuccess();
    }

    [McpServerTool, Description("Adds test cases to a suite.")]
    public static async Task<bool> AddTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets the root suite of a test plan.")]
    public static async Task<TestSuite> GetRootSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int planId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetRootSuiteAsync(planId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists test cases in a suite.")]
    public static async Task<PagedList<TestCase>> ListTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListTestCasesAsync(testPlanId, testSuiteId)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets test results for a build.")]
    public static async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestResultsDetails> GetTestResultsForBuildAsync(string organizationUrl, string projectName, string personalAccessToken, int buildId)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetTestResultsForBuildAsync(projectName, buildId)).EnsureSuccess();
    }
}