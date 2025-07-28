using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public interface IWikiClient
    {
        Task<int?> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions wikiPageUpdateOptions, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions pagesOptions, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default);

        Task<string?> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default);

        Task<string> SearchWikiAsync(WikiSearchOptions searchOptions, CancellationToken cancellationToken = default);

        Task<Guid> CreateWikiAsync(WikiCreateOptions wikiCreateOptions, CancellationToken cancellationToken = default);

        Task DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default);

        Task DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default);

        Task<WikiPageResponse?> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default);

        Task<WikiV2?> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WikiV2>> ListWikisAsync(CancellationToken cancellationToken = default);

    }
}
