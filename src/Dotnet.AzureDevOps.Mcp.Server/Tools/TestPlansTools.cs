using System.ComponentModel;
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
    private TestPlansClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a test plan.")]
    public Task<int> CreateTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, TestPlanCreateOptions options)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateTestPlanAsync(options);
    }

    [McpServerTool, Description("Retrieves a test plan.")]
    public Task<TestPlan?> GetTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetTestPlanAsync(testPlanId);
    }

    [McpServerTool, Description("Lists test plans.")]
    public Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListTestPlansAsync();
    }

    [McpServerTool, Description("Deletes a test plan.")]
    public Task DeleteTestPlanAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteTestPlanAsync(testPlanId);
    }

    [McpServerTool, Description("Creates a test suite.")]
    public Task<int> CreateTestSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, TestSuiteCreateOptions options)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateTestSuiteAsync(testPlanId, options);
    }

    [McpServerTool, Description("Lists test suites for a plan.")]
    public Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListTestSuitesAsync(testPlanId);
    }

    [McpServerTool, Description("Adds test cases to a suite.")]
    public Task AddTestCasesAsync(string organizationUrl, string projectName, string personalAccessToken, int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.AddTestCasesAsync(testPlanId, testSuiteId, testCaseIds);
    }

    [McpServerTool, Description("Gets the root suite of a test plan.")]
    public Task<TestSuite> GetRootSuiteAsync(string organizationUrl, string projectName, string personalAccessToken, int planId)
    {
        TestPlansClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetRootSuiteAsync(planId);
    }
}
