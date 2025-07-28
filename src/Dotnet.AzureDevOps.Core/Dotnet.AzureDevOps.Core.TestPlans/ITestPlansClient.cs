using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public interface ITestPlansClient
{
    Task<int> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default);

    Task<TestPlan?> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(CancellationToken cancellationToken = default);

    Task DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);

    Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default);

    Task AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default);

    Task<WorkItem?> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItem>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default);

    Task<TestResultsDetails?> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default);
}
