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
}
