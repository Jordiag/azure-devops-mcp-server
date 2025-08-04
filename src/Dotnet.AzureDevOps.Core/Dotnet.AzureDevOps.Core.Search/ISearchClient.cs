using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Search.Options;

namespace Dotnet.AzureDevOps.Core.Search;

public interface ISearchClient
{
    Task<AzureDevOpsActionResult<string>> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<string>> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<string>> SearchWorkItemsAsync(WorkItemSearchOptions options, CancellationToken cancellationToken = default);
    Task<AzureDevOpsActionResult<bool>> IsCodeSearchEnabledAsync(CancellationToken cancellationToken = default);
}
