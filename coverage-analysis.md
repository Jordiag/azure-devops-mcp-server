# Integration Test Coverage Analysis

This analysis compares the public interface methods in each Core library with what's covered in integration tests.

## Boards (IWorkItemsClient)

### Interface Methods (Total: ~42 methods):
- AddAttachmentAsync
- AddCommentAsync  
- AddLinkAsync
- BulkUpdateWorkItemsAsync
- CreateEpicAsync ✅
- CreateFeatureAsync ✅
- CreateTaskAsync ✅
- CreateUserStoryAsync ✅
- DeleteWorkItemAsync
- ExportBoardAsync ✅
- GetAttachmentAsync
- GetCommentsAsync
- GetCustomFieldAsync
- GetHistoryAsync
- GetLinksAsync
- ListBoardsAsync ✅
- GetTeamIterationAsync
- GetTeamIterationsAsync
- GetWorkItemAsync ✅
- GetWorkItemCountAsync
- ListAreasAsync
- ListBoardColumnsAsync ✅
- ListBacklogsAsync
- ListBacklogWorkItemsAsync
- ListMyWorkItemsAsync
- LinkWorkItemToPullRequestAsync ✅
- GetWorkItemsForIterationAsync
- ListIterationsAsync
- CreateIterationsAsync
- AssignIterationsAsync
- QueryWorkItemsAsync ✅
- RemoveLinkAsync
- SetCustomFieldAsync
- UpdateEpicAsync ✅
- UpdateFeatureAsync ✅
- UpdateTaskAsync
- UpdateUserStoryAsync ✅
- ExecuteBatchAsync
- CreateWorkItemsMultipleCallsAsync
- UpdateWorkItemsBatchAsync
- LinkWorkItemsBatchAsync
- CloseWorkItemsBatchAsync
- CloseAndLinkDuplicatesBatchAsync
- GetWorkItemsBatchByIdsAsync
- LinkWorkItemsByNameBatchAsync

### Integration Test Coverage: ~15/42 methods (36%)

### Missing from Integration Tests:
- AddAttachmentAsync
- AddCommentAsync
- AddLinkAsync  
- BulkUpdateWorkItemsAsync
- DeleteWorkItemAsync
- GetAttachmentAsync
- GetCommentsAsync
- GetCustomFieldAsync
- GetHistoryAsync
- GetLinksAsync
- GetTeamIterationAsync
- GetTeamIterationsAsync
- GetWorkItemCountAsync
- ListAreasAsync
- ListBacklogsAsync
- ListBacklogWorkItemsAsync
- ListMyWorkItemsAsync
- GetWorkItemsForIterationAsync
- ListIterationsAsync
- CreateIterationsAsync
- AssignIterationsAsync
- RemoveLinkAsync
- SetCustomFieldAsync
- UpdateTaskAsync
- ExecuteBatchAsync
- CreateWorkItemsMultipleCallsAsync
- UpdateWorkItemsBatchAsync
- LinkWorkItemsBatchAsync
- CloseWorkItemsBatchAsync
- CloseAndLinkDuplicatesBatchAsync
- GetWorkItemsBatchByIdsAsync
- LinkWorkItemsByNameBatchAsync

## TestPlans (ITestPlansClient)

### Interface Methods (Total: 11 methods):
- AddTestCasesAsync
- CreateTestCaseAsync ✅
- CreateTestPlanAsync ✅
- CreateTestSuiteAsync ✅
- DeleteTestPlanAsync ✅
- GetRootSuiteAsync ✅
- GetTestPlanAsync ✅
- GetTestResultsForBuildAsync
- ListTestCasesAsync ✅
- ListTestPlansAsync ✅
- ListTestSuitesAsync ✅

### Integration Test Coverage: ~9/11 methods (82%)

### Missing from Integration Tests:
- AddTestCasesAsync
- GetTestResultsForBuildAsync

## Summary

TestPlans has excellent coverage (82%) while Boards has significant gaps (36% coverage). The missing methods in Boards include important functionality like:
- Attachment management
- Comment management  
- Link management
- Batch operations
- Custom field operations
- Area/iteration management
- History tracking

Similar analysis should be done for other Core libraries (Repos, Pipelines, Artifacts, Overview, ProjectSettings, Search).

## Public Methods Not Declared in Interfaces

### Boards (WorkItemsClient)
The following public methods exist in the implementation but are **NOT** declared in `IWorkItemsClient`:

1. **`GetWorkItemTypeAsync`** - Retrieves work item type information
   - Location: `WorkItemsClient.Queries.cs` line 83
   - Signature: `Task<AzureDevOpsActionResult<WorkItemType>> GetWorkItemTypeAsync(string projectName, string workItemTypeName, CancellationToken cancellationToken = default)`

2. **`GetQueryAsync`** - Gets query hierarchy information  
   - Location: `WorkItemsClient.Queries.cs` line 96
   - Signature: `Task<AzureDevOpsActionResult<QueryHierarchyItem>> GetQueryAsync(string projectName, string queryIdOrPath, QueryExpand? expand = null, int depth = 0, bool includeDeleted = false, bool useIsoDateFormat = false, CancellationToken cancellationToken = default)`

3. **`GetQueryResultsByIdAsync`** - Gets query results by query ID
   - Location: `WorkItemsClient.Queries.cs` line 109
   - Signature: `Task<AzureDevOpsActionResult<WorkItemQueryResult>> GetQueryResultsByIdAsync(Guid queryId, TeamContext teamContext, bool? timePrecision = false, int top = 50, CancellationToken cancellationToken = default)`

4. **`CreateSharedQueryAsync`** - Creates shared queries
   - Location: `WorkItemsClient.Queries.cs` line 122
   - Signature: `Task<AzureDevOpsActionResult<bool>> CreateSharedQueryAsync(string projectName, string queryName, string wiql, CancellationToken cancellationToken = default)`

5. **`DeleteSharedQueryAsync`** - Deletes shared queries
   - Location: `WorkItemsClient.Queries.cs` line 158
   - Signature: `Task<AzureDevOpsActionResult<bool>> DeleteSharedQueryAsync(string projectName, string queryName, CancellationToken cancellationToken = default)`

6. **`IsSystemProcessAsync`** - Checks if project uses system process
   - Location: `WorkItemsClient.cs` line 44
   - Signature: `Task<AzureDevOpsActionResult<bool>> IsSystemProcessAsync(CancellationToken cancellationToken = default)`

### Artifacts (ArtifactsClient)
The following methods have **signature mismatches** between interface and implementation:

1. **`SetRetentionPolicyAsync`** - Interface vs Implementation mismatch
   - Interface: `Task<AzureDevOpsActionResult<FeedRetentionPolicy>> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default)`
   - Implementation: `Task<AzureDevOpsActionResult<RetentionPolicyResult>> SetRetentionPolicyAsync(Guid feedId, int daysToKeep, string[] packageTypes, CancellationToken cancellationToken = default)`

### Impact
These missing interface declarations mean:
- Methods cannot be properly mocked/tested with interface-based dependency injection
- Methods are not discoverable through the interface contract
- Breaking the interface segregation principle
- Some methods mentioned in TODO.md are acknowledged as needing to be added to MCP server tools

### Resolution Required
1. Add missing method declarations to respective interfaces
2. Fix signature mismatches in Artifacts
3. Update MCP server tools to expose the missing methods (as noted in TODO.md)
