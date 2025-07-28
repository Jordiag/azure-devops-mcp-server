using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans
{
    public interface ITestPlansClient
    {
        Task AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default);
        Task<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem?> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default);
        Task<int> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default);
        Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default);
        Task DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);
        Task<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite> GetRootSuiteAsync(int planId);
        Task<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan?> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);
        Task<TestResultsDetails?> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan>> ListTestPlansAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default);
    }
}