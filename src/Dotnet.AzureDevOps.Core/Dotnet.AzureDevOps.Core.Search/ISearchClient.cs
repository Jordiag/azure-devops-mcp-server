using Dotnet.AzureDevOps.Core.Search.Options;

namespace Dotnet.AzureDevOps.Core.Search;

public interface ISearchClient
{
    Task<string> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default);
    Task<string> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default);
    Task<string> SearchWorkItemsAsync(WorkItemSearchOptions options, CancellationToken cancellationToken = default);
}
