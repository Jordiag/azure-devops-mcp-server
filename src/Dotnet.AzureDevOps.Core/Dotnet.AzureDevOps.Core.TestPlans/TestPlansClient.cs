using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.TestResults.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using WorkItem = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public class TestPlansClient : AzureDevOpsClientBase, ITestPlansClient
{
    private readonly TestPlanHttpClient _testPlanClient;

    public TestPlansClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        : base(organizationUrl, personalAccessToken, projectName, logger)
    {
        _testPlanClient = Connection.GetClient<TestPlanHttpClient>();
    }

    /// <summary>
    /// Creates a new test plan in Azure DevOps with the specified configuration including name, area path, iteration, and schedule.
    /// Test plans serve as containers for organizing test suites and test cases, enabling structured test execution planning and tracking.
    /// Supports comprehensive test management workflow including area-based organization, iteration-based scheduling, and descriptive documentation.
    /// Essential for establishing test execution frameworks and quality assurance processes within Azure DevOps projects.
    /// </summary>
    /// <param name="testPlanCreateOptions">Test plan configuration including name, area path, iteration, start/end dates, and description for comprehensive test organization and scheduling.</param>
    /// <param name="cancellationToken">Optional token to cancel the test plan creation operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Integer ID of the newly created test plan for future reference and management
    /// - Failure: Error details if test plan creation fails due to invalid parameters, permissions, or service issues
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when required test plan parameters like name or area path are missing or invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to create test plans in the specified project</exception>
    /// <exception cref="VssServiceException">Thrown when Azure DevOps service encounters issues during test plan creation or validation</exception>
    public async Task<AzureDevOpsActionResult<int>> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            var createParameters = new TestPlanCreateParams
            {
                Name = testPlanCreateOptions.Name,
                AreaPath = testPlanCreateOptions.AreaPath,
                Iteration = testPlanCreateOptions.Iteration,
                StartDate = testPlanCreateOptions.StartDate,
                EndDate = testPlanCreateOptions.EndDate,
                Description = testPlanCreateOptions.Description
            };

            TestPlan plan = await _testPlanClient.CreateTestPlanAsync(
                testPlanCreateParams: createParameters,
                project: ProjectName,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<int>.Success(plan.Id, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a specific test plan from Azure DevOps by its unique identifier, including all associated metadata and configuration.
    /// Provides comprehensive test plan information including name, area path, iteration, schedule, description, and organizational structure.
    /// Essential for test plan inspection, validation, and management operations within Azure DevOps test management workflows.
    /// Enables programmatic access to test plan details for reporting, analysis, and automated test execution planning.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the test plan to retrieve from Azure DevOps test management system.</param>
    /// <param name="cancellationToken">Optional token to cancel the test plan retrieval operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: TestPlan object with complete plan details including metadata, configuration, and organizational information
    /// - Failure: Error details if test plan is not found, access is denied, or service issues occur during retrieval
    /// </returns>
    /// <exception cref="VssServiceException">Thrown when the specified test plan ID does not exist or cannot be accessed</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to view the test plan or project</exception>
    /// <exception cref="ArgumentException">Thrown when the test plan ID is invalid or malformed</exception>
    public async Task<AzureDevOpsActionResult<TestPlan>> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            TestPlan plan = await _testPlanClient.GetTestPlanByIdAsync(
                project: ProjectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<TestPlan>.Success(plan, Logger);
        }
        catch(VssServiceException)
        {
            return AzureDevOpsActionResult<TestPlan>.Failure("Test plan is not found", Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestPlan>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves all test plans available in the Azure DevOps project, providing comprehensive overview of test management structure.
    /// Returns paginated collection of test plans with their metadata, configurations, and organizational details for project-wide test visibility.
    /// Essential for test management dashboard creation, project oversight, and comprehensive test planning across multiple initiatives.
    /// Enables discovery and analysis of all test planning activities within the project scope for better test coordination and resource management.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the test plans listing operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Read-only list of all TestPlan objects with complete metadata and configuration details for the project
    /// - Failure: Error details if test plans cannot be retrieved due to permissions, service issues, or project access problems
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to list test plans in the specified project</exception>
    /// <exception cref="VssServiceException">Thrown when Azure DevOps service encounters issues during test plans retrieval or project validation</exception>
    /// <exception cref="TimeoutException">Thrown when the operation exceeds allowed time limits due to large datasets or service performance issues</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestPlan>>> ListTestPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestPlan> plans = await _testPlanClient.GetTestPlansAsync(
                project: ProjectName,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Success(plans, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Permanently deletes a test plan from Azure DevOps, removing all associated test suites, test cases, and execution history.
    /// Performs irreversible removal of the entire test plan structure including all child elements and relationships.
    /// Critical operation for test plan lifecycle management and cleanup of obsolete or incorrect test planning artifacts.
    /// Should be used with caution as deletion cannot be undone and will impact any dependent test execution processes.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the test plan to permanently delete from Azure DevOps test management system.</param>
    /// <param name="cancellationToken">Optional token to cancel the test plan deletion operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Boolean true indicating successful deletion of the test plan and all associated elements
    /// - Failure: Error details if deletion fails due to permissions, dependencies, or service issues during removal
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to delete the test plan or modify the project</exception>
    /// <exception cref="VssServiceException">Thrown when the test plan cannot be found or deletion is blocked by system constraints</exception>
    /// <exception cref="InvalidOperationException">Thrown when the test plan is in use by active test runs or has dependencies that prevent deletion</exception>
    public async Task<AzureDevOpsActionResult<bool>> DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _testPlanClient.DeleteTestPlanAsync(
                project: ProjectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<bool>.Success(true, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Creates a new test suite within an existing test plan, establishing a structured container for organizing related test cases.
    /// Test suites provide hierarchical organization of test cases enabling logical grouping by feature, component, or testing approach.
    /// Supports static test suite creation with configurable parent-child relationships for comprehensive test organization.
    /// Essential for maintaining organized test execution structure and enabling efficient test case management within test plans.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the parent test plan where the new test suite will be created and organized.</param>
    /// <param name="testSuiteCreateOptions">Test suite configuration including name, parent suite reference, and organizational parameters for proper test structure.</param>
    /// <param name="cancellationToken">Optional token to cancel the test suite creation operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Integer ID of the newly created test suite for future test case assignment and management
    /// - Failure: Error details if test suite creation fails due to invalid parameters, permissions, or parent plan issues
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when required test suite parameters like name or parent suite are missing or invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to create test suites in the specified test plan</exception>
    /// <exception cref="VssServiceException">Thrown when the parent test plan does not exist or Azure DevOps service encounters creation issues</exception>
    public async Task<AzureDevOpsActionResult<int>> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            var createParameters = new TestSuiteCreateParams
            {
                Name = testSuiteCreateOptions.Name,
                SuiteType = TestSuiteType.StaticTestSuite,
                ParentSuite = testSuiteCreateOptions.ParentSuite
            };

            TestSuite suite = await _testPlanClient.CreateTestSuiteAsync(
                testSuiteCreateParams: createParameters,
                project: ProjectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<int>.Success(suite.Id, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves all test suites within a specified test plan, providing complete hierarchical structure and organizational details.
    /// Returns flat list of all test suites including root and child suites with their metadata, relationships, and configuration information.
    /// Essential for understanding test plan organization, suite hierarchy navigation, and comprehensive test structure analysis.
    /// Enables programmatic access to test suite structure for reporting, automation, and test execution planning workflows.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the test plan from which to retrieve all associated test suites and their details.</param>
    /// <param name="cancellationToken">Optional token to cancel the test suites listing operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Read-only list of all TestSuite objects with complete metadata, hierarchy information, and organizational structure
    /// - Failure: Error details if test suites cannot be retrieved due to permissions, invalid test plan ID, or service issues
    /// </returns>
    /// <exception cref="VssServiceException">Thrown when the specified test plan ID does not exist or cannot be accessed for suite enumeration</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to view test suites within the specified test plan</exception>
    /// <exception cref="ArgumentException">Thrown when the test plan ID is invalid or malformed</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestSuite>>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestSuite> suites = await _testPlanClient.GetTestSuitesForPlanAsync(
                project: ProjectName,
                planId: testPlanId,
                asTreeView: false,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Success(suites, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Associates existing test case work items with a specific test suite, establishing test execution relationships and coverage.
    /// Enables bulk assignment of multiple test cases to test suites for efficient test organization and execution planning.
    /// Creates point assignments and test case references within the suite structure enabling comprehensive test execution tracking.
    /// Essential for building test execution scope and establishing traceability between test cases and test execution plans.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the test plan containing the target test suite for test case assignment.</param>
    /// <param name="testSuiteId">Unique identifier of the test suite where the test cases will be added and organized.</param>
    /// <param name="testCaseIds">Collection of test case work item IDs to be associated with the specified test suite for execution.</param>
    /// <param name="cancellationToken">Optional token to cancel the test case addition operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: Boolean true indicating successful addition of all test cases to the test suite with proper point assignments
    /// - Failure: Error details if test case addition fails due to invalid IDs, permissions, or suite configuration issues
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when test case IDs are invalid, duplicate, or reference non-existent work items</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to modify test suites or access test cases</exception>
    /// <exception cref="VssServiceException">Thrown when test plan or test suite cannot be found or service encounters assignment issues</exception>
    public async Task<AzureDevOpsActionResult<bool>> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default)
    {
        try
        {
            List<WorkItem> references = testCaseIds.Select(id => new WorkItem { Id = id }).ToList();
            var existingTestCases = new List<SuiteTestCaseCreateUpdateParameters>();

            foreach(WorkItem workItem in references)
            {
                var suiteTestCase = new SuiteTestCaseCreateUpdateParameters
                {
                    workItem = new WorkItem { Id = workItem.Id },
                    PointAssignments = []
                };
                existingTestCases.Add(suiteTestCase);
            }

            await _testPlanClient.AddTestCasesToSuiteAsync(
                suiteTestCaseCreateUpdateParameters: existingTestCases,
                project: ProjectName,
                planId: testPlanId,
                suiteId: testSuiteId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<bool>.Success(true, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Creates a new test case work item in Azure DevOps with comprehensive test specifications including steps, priority, and organizational metadata.
    /// Establishes structured test case with configurable test steps, priority levels, area path assignment, and iteration planning integration.
    /// Supports rich test case definition including step-by-step test procedures, expected results, and quality assurance parameters.
    /// Essential for building comprehensive test coverage and establishing detailed test execution specifications within Azure DevOps projects.
    /// </summary>
    /// <param name="options">Test case configuration including title, test steps, priority, area path, iteration path, and project assignment for complete test specification.</param>
    /// <param name="cancellationToken">Optional token to cancel the test case creation operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: WorkItem object representing the newly created test case with all metadata, fields, and system-generated properties
    /// - Failure: Error details if test case creation fails due to invalid parameters, permissions, or work item tracking issues
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when required test case parameters like title or project are missing or invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to create work items in the specified project</exception>
    /// <exception cref="VssServiceException">Thrown when Azure DevOps work item tracking service encounters issues during test case creation</exception>
    public async Task<AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            using var workItemTracking = new WorkItemTrackingHttpClient(Connection.Uri, Connection.Credentials);

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = options.Title }
            };

            if(!string.IsNullOrWhiteSpace(options.Steps))
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.TCM.Steps",
                    Value = options.Steps
                });
            }

            if(options.Priority.HasValue)
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Priority",
                    Value = options.Priority.Value
                });
            }

            if(!string.IsNullOrWhiteSpace(options.AreaPath))
            {
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = options.AreaPath });
            }

            if(!string.IsNullOrWhiteSpace(options.IterationPath))
            {
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = options.IterationPath });
            }

            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem result = await workItemTracking.CreateWorkItemAsync(
                patch,
                options.Project,
                "Test Case",
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves all test cases associated with a specific test suite within a test plan, providing comprehensive test execution scope.
    /// Returns paginated collection of test cases with their metadata, configuration, and execution status for suite-specific test management.
    /// Essential for understanding test suite coverage, test case distribution, and execution planning within specific organizational units.
    /// Enables targeted test case analysis, execution preparation, and comprehensive test coverage assessment for specific test suites.
    /// </summary>
    /// <param name="testPlanId">Unique identifier of the test plan containing the target test suite for test case enumeration.</param>
    /// <param name="testSuiteId">Unique identifier of the test suite from which to retrieve all associated test cases and their details.</param>
    /// <param name="cancellationToken">Optional token to cancel the test cases listing operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: PagedList of TestCase objects with complete metadata, execution status, and suite-specific configuration details
    /// - Failure: Error details if test cases cannot be retrieved due to permissions, invalid identifiers, or service issues
    /// </returns>
    /// <exception cref="VssServiceException">Thrown when the specified test plan or test suite IDs do not exist or cannot be accessed</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to view test cases within the specified test suite</exception>
    /// <exception cref="ArgumentException">Thrown when test plan or test suite IDs are invalid or malformed</exception>
    public async Task<AzureDevOpsActionResult<PagedList<TestCase>>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestCase> testCases = await _testPlanClient.GetTestCaseListAsync(
                ProjectName,
                testPlanId,
                testSuiteId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<PagedList<TestCase>>.Success(testCases, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<PagedList<TestCase>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves comprehensive test execution results and statistics for a specific build in Azure DevOps, providing complete test outcome analysis.
    /// Returns detailed test results including pass/fail counts, execution duration, test case outcomes, and build-specific quality metrics.
    /// Essential for continuous integration quality gates, build validation, and comprehensive test execution reporting across build pipelines.
    /// Enables automated quality assessment, test trend analysis, and build quality verification for development and deployment workflows.
    /// </summary>
    /// <param name="projectName">Name of the Azure DevOps project containing the build for which test results should be retrieved and analyzed.</param>
    /// <param name="buildId">Unique identifier of the build for which comprehensive test execution results and statistics are needed.</param>
    /// <param name="cancellationToken">Optional token to cancel the test results retrieval operation if needed before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: TestResultsDetails object with complete test execution statistics, outcomes, duration, and build-specific quality metrics
    /// - Failure: Error details if test results cannot be retrieved due to permissions, invalid build ID, or service availability issues
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when project name is invalid or build ID does not exist in the specified project</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access test results for the specified build</exception>
    /// <exception cref="VssServiceException">Thrown when Azure DevOps test results service encounters issues during results retrieval</exception>
    public async Task<AzureDevOpsActionResult<TestResultsDetails>> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            TestResultsHttpClient testResultsClient = await Connection.GetClientAsync<TestResultsHttpClient>(cancellationToken);
            TestResultsDetails details = await testResultsClient.GetTestResultDetailsForBuildAsync(
                projectName,
                buildId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<TestResultsDetails>.Success(details, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestResultsDetails>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Locates and retrieves the root test suite for a specified test plan, providing access to the top-level organizational container.
    /// Root suites serve as the primary organizational unit containing all other test suites and test cases within the test plan hierarchy.
    /// Essential for test plan navigation, hierarchical test organization, and programmatic access to complete test suite structures.
    /// Enables automated test suite discovery and provides entry point for comprehensive test plan traversal and management operations.
    /// </summary>
    /// <param name="planId">Unique identifier of the test plan for which the root test suite should be located and retrieved.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: TestSuite object representing the root suite with complete metadata and hierarchical organization details
    /// - Failure: Error details if root suite cannot be found, test plan is invalid, or service issues occur during retrieval
    /// </returns>
    /// <exception cref="VssServiceException">Thrown when the specified test plan ID does not exist or cannot be accessed for suite enumeration</exception>
    /// <exception cref="InvalidOperationException">Thrown when no root suite exists for the test plan or suite hierarchy is malformed</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access test suites within the specified test plan</exception>
    public async Task<AzureDevOpsActionResult<TestSuite>> GetRootSuiteAsync(int planId, CancellationToken cancellationToken = default)
    {
        try
        {
            AzureDevOpsActionResult<IReadOnlyList<TestSuite>> suitesResult = await ListTestSuitesAsync(planId, cancellationToken);
            if(!suitesResult.IsSuccessful || suitesResult.Value == null)
                return AzureDevOpsActionResult<TestSuite>.Failure(suitesResult.ErrorMessage ?? $"Unable to list suites for plan {planId}.", Logger);

            IReadOnlyList<TestSuite> suites = suitesResult.Value;
            TestSuite? root = suites.FirstOrDefault(suite => suite.ParentSuite == null || suite.ParentSuite.Id != -1);
            return root is null
                ? AzureDevOpsActionResult<TestSuite>.Failure($"No root suite found for test plan {planId}.", Logger)
                : AzureDevOpsActionResult<TestSuite>.Success(root, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestSuite>.Failure(ex, Logger);
        }
    }


}
