using Microsoft.TeamFoundation.SourceControl.WebApi;
using Xunit;

namespace Dotnet.AzureDevOps.Tests.Common;

/// <summary>
/// Base class for integration tests that provides common resource tracking and cleanup functionality.
/// </summary>
public abstract class BaseIntegrationTestFixture(IntegrationTestFixture fixture) : IAsyncLifetime
{
    protected readonly IntegrationTestFixture Fixture = fixture;
    protected readonly AzureDevOpsConfiguration Configuration = fixture.Configuration;

    // Resource tracking collections
    private readonly List<int> _createdWorkItemIds = [];
    private readonly List<int> _createdPullRequestIds = [];
    private readonly List<Guid> _createdWikiIds = [];
    private readonly List<int> _createdTestPlanIds = [];
    private readonly List<int> _createdBuildIds = [];
    private readonly List<int> _createdDefinitionIds = [];
    private readonly List<Guid> _createdProjectIds = [];

    /// <summary>
    /// Registers a work item ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedWorkItem(int workItemId)
    {
        _createdWorkItemIds.Add(workItemId);
    }

    /// <summary>
    /// Registers a pull request ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedPullRequest(int pullRequestId)
    {
        _createdPullRequestIds.Add(pullRequestId);
    }

    /// <summary>
    /// Registers a wiki ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedWiki(Guid wikiId)
    {
        _createdWikiIds.Add(wikiId);
    }

    /// <summary>
    /// Registers a test plan ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedTestPlan(int testPlanId)
    {
        _createdTestPlanIds.Add(testPlanId);
    }

    /// <summary>
    /// Registers a build ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedBuild(int buildId)
    {
        _createdBuildIds.Add(buildId);
    }

    /// <summary>
    /// Registers a build definition ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedDefinition(int definitionId)
    {
        _createdDefinitionIds.Add(definitionId);
    }

    /// <summary>
    /// Registers a project ID for cleanup during test disposal.
    /// </summary>
    protected void RegisterCreatedProject(Guid projectId)
    {
        _createdProjectIds.Add(projectId);
    }

    /// <summary>
    /// Removes a resource from tracking (useful when resource is deleted within a test).
    /// </summary>
    protected void UnregisterCreatedWiki(Guid wikiId)
    {
        _createdWikiIds.Remove(wikiId);
    }

    /// <summary>
    /// Removes a resource from tracking (useful when resource is deleted within a test).
    /// </summary>
    protected void UnregisterCreatedTestPlan(int testPlanId)
    {
        _createdTestPlanIds.Remove(testPlanId);
    }

    /// <summary>
    /// Removes a resource from tracking (useful when resource is deleted within a test).
    /// </summary>
    protected void UnregisterCreatedDefinition(int definitionId)
    {
        _createdDefinitionIds.Remove(definitionId);
    }

    /// <summary>
    /// Generates a UTC timestamp in ISO format suitable for resource naming.
    /// </summary>
    protected static string UtcStamp() => DateTime.UtcNow.ToString("O").Replace(':', '-');

    /// <summary>
    /// Generates a UTC timestamp in yyyyMMddHHmmss format suitable for resource naming.
    /// </summary>
    protected static string UtcStampShort() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");

    /// <summary>
    /// Generates a unique test-specific identifier with optional prefix.
    /// </summary>
    protected static string GenerateTestId(string prefix = "test") => $"{prefix}-{UtcStampShort()}";

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await CleanupResourcesAsync();
    }

    /// <summary>
    /// Cleans up all registered resources in reverse order of creation.
    /// Override this method to add custom cleanup logic.
    /// </summary>
    protected virtual async Task CleanupResourcesAsync()
    {
        // Clean up projects (usually created last, cleaned up first)
        await CleanupProjectsAsync();

        // Clean up build definitions
        await CleanupBuildDefinitionsAsync();

        // Clean up builds (usually cancel running builds)
        await CleanupBuildsAsync();

        // Clean up test plans
        await CleanupTestPlansAsync();

        // Clean up wikis
        await CleanupWikisAsync();

        // Clean up pull requests (abandon if not completed)
        await CleanupPullRequestsAsync();

        // Clean up work items (delete)
        await CleanupWorkItemsAsync();
    }

    private async Task CleanupWorkItemsAsync()
    {
        foreach(int id in _createdWorkItemIds.AsEnumerable().Reverse())
        {
            try
            {
                await Fixture.WorkItemsClient.DeleteWorkItemAsync(id);
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupPullRequestsAsync()
    {
        foreach(int id in _createdPullRequestIds.AsEnumerable().Reverse())
        {
            try
            {
                Core.Common.AzureDevOpsActionResult<GitPullRequest> prResult = await Fixture.ReposClient.GetPullRequestAsync(Configuration.RepoName, id);
                GitPullRequest pr = prResult.Value;
                if(pr != null && pr.Status != PullRequestStatus.Completed)
                {
                    await Fixture.ReposClient.AbandonPullRequestAsync(Configuration.RepoName, id);
                }
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupWikisAsync()
    {
        foreach(Guid id in _createdWikiIds.AsEnumerable().Reverse())
        {
            try
            {
                await Fixture.WikiClient.DeleteWikiAsync(id);
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupTestPlansAsync()
    {
        foreach(int id in _createdTestPlanIds.AsEnumerable().Reverse())
        {
            try
            {
                await Fixture.TestPlansClient.DeleteTestPlanAsync(id);
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupBuildsAsync()
    {
        foreach(int id in _createdBuildIds.AsEnumerable().Reverse())
        {
            try
            {
                Core.Common.AzureDevOpsActionResult<Microsoft.TeamFoundation.Build.WebApi.Build> buildResult = await Fixture.PipelinesClient.GetRunAsync(id);
                Microsoft.TeamFoundation.Build.WebApi.Build build = buildResult.Value;
                if(build != null)
                {
                    await Fixture.PipelinesClient.CancelRunAsync(id, build.Project);
                }
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupBuildDefinitionsAsync()
    {
        foreach(int id in _createdDefinitionIds.AsEnumerable().Reverse())
        {
            try
            {
                await Fixture.PipelinesClient.DeletePipelineAsync(id);
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }

    private async Task CleanupProjectsAsync()
    {
        foreach(Guid id in _createdProjectIds.AsEnumerable().Reverse())
        {
            try
            {
                await Fixture.ProjectSettingsClient.DeleteProjectAsync(id);
            }
            catch(Exception)
            {
                // Ignore cleanup failures to avoid masking test failures
            }
        }
    }
}
