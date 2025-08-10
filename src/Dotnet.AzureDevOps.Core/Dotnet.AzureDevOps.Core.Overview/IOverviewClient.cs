using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public interface IOverviewClient : IDisposable, IAsyncDisposable
    {
        // Dashboard methods
        Task<AzureDevOpsActionResult<IReadOnlyList<Dashboard>>> ListDashboardsAsync(CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<Dashboard>> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default);

        // Summary methods
        Task<AzureDevOpsActionResult<TeamProject>> GetProjectSummaryAsync(CancellationToken cancellationToken = default);

        // Wiki methods
        Task<AzureDevOpsActionResult<Guid>> CreateWikiAsync(WikiCreateOptions wikiCreateOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<int>> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions wikiPageUpdateOptions, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions pagesOptions, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<string>> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<WikiPageResponse>> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<WikiV2>> DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<WikiPageResponse>> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<WikiV2>> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<WikiV2>>> ListWikisAsync(CancellationToken cancellationToken = default);
    }
}
