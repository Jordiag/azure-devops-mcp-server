using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public interface ITestPlansClient : IDisposable, IAsyncDisposable
{
    Task<AzureDevOpsActionResult<bool>> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<int>> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite>> GetRootSuiteAsync(int planId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan>> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<TestResultsDetails>> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<PagedList<TestCase>>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan>>> ListTestPlansAsync(CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite>>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default);
}