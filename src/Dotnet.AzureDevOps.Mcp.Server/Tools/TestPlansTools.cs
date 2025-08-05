using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
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
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<int> result = await client.CreateTestPlanAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create test plan.");
        return result.Value;
    }

    [McpServerTool, Description("Retrieves a test plan.")]
    public static async Task<TestPlan?> GetTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<TestPlan> result = await client.GetTestPlanAsync(testPlanId);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Lists test plans.")]
    public static async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<TestPlan>> result = await client.ListTestPlansAsync();
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list test plans.");
        return result.Value ?? Array.Empty<TestPlan>();
    }

    [McpServerTool, Description("Deletes a test plan.")]
    public static async Task DeleteTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.DeleteTestPlanAsync(testPlanId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete test plan.");
    }

    [McpServerTool, Description("Creates a test suite.")]
    public static async Task<int> CreateTestSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, TestSuiteCreateOptions options)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<int> result = await client.CreateTestSuiteAsync(testPlanId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create test suite.");
        return result.Value;
    }

    [McpServerTool, Description("Lists test suites for a plan.")]
    public static async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite>> result = await client.ListTestSuitesAsync(testPlanId);
        if (!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list test suites.");
        return result.Value ?? Array.Empty<TestSuite>();
    }

    [McpServerTool, Description("Adds test cases to a suite.")]
    public static async Task AddTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<bool> result = await client.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to add test cases.");
    }

    [McpServerTool, Description("Gets the root suite of a test plan.")]
    public static async Task<TestSuite?> GetRootSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int planId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        AzureDevOpsActionResult<TestSuite> result = await client.GetRootSuiteAsync(planId);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }
}
