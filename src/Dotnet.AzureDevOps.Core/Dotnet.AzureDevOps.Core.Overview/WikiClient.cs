using System.Text;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class WikiClient : IWikiClient
    {
        private readonly string _projectName;
        private readonly WikiHttpClient _wikiHttpClient;
        private readonly ILogger? _logger;

        public WikiClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _logger = logger;

            VssBasicCredential credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            VssConnection connection = new VssConnection(new Uri(organizationUrl), credentials);
            _wikiHttpClient = connection.GetClient<WikiHttpClient>();
        }

        public async Task<AzureDevOpsActionResult<Guid>> CreateWikiAsync(WikiCreateOptions wikiCreateOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiCreateParametersV2 wikiCreateParameters = new WikiCreateParametersV2
                {
                    Name = wikiCreateOptions.Name,
                    ProjectId = wikiCreateOptions.ProjectId,
                    RepositoryId = wikiCreateOptions.RepositoryId,
                    Type = wikiCreateOptions.Type,
                    MappedPath = wikiCreateOptions.MappedPath,
                    Version = wikiCreateOptions.Version,
                };

                WikiV2 wiki = await _wikiHttpClient.CreateWikiAsync(
                    wikiCreateParams: wikiCreateParameters,
                    project: _projectName,
                    cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Guid>.Success(wiki.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<WikiV2>> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiV2 wiki = await _wikiHttpClient.GetWikiAsync(project: _projectName, wikiIdentifier: wikiId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiV2>.Success(wiki, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiV2>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WikiV2>>> ListWikisAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<WikiV2> wikis = await _wikiHttpClient.GetAllWikisAsync(project: _projectName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WikiV2>>.Success(wikis, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WikiV2>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<WikiV2>> DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiV2 wiki = await _wikiHttpClient.DeleteWikiAsync(wikiIdentifier: wikiId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiV2>.Success(wiki, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiV2>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions wikiPageUpdateOptions, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPageCreateOrUpdateParameters pageParameters = new WikiPageCreateOrUpdateParameters
                {
                    Content = wikiPageUpdateOptions.Content
                };

                WikiPageResponse response = await _wikiHttpClient.CreateOrUpdatePageAsync(
                    parameters: pageParameters,
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: wikiPageUpdateOptions.Path,
                    Version: wikiPageUpdateOptions.Version,
                    versionDescriptor: gitVersionDescriptor,
                    cancellationToken: cancellationToken);

                int? pageId = response.Page?.Id;
                return pageId.HasValue
                    ? AzureDevOpsActionResult<int>.Success(pageId.Value, _logger)
                    : AzureDevOpsActionResult<int>.Failure("Wiki page id is null.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<WikiPageResponse>> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPageResponse response = await _wikiHttpClient.GetPageAsync(
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: path,
                    includeContent: true,
                    cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiPageResponse>.Success(response, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiPageResponse>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions pagesOptions, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPagesBatchRequest request = new WikiPagesBatchRequest
                {
                    Top = pagesOptions.Top,
                    ContinuationToken = pagesOptions.ContinuationToken,
                    PageViewsForDays = pagesOptions.PageViewsForDays
                };

                PagedList<WikiPageDetail> pages = await _wikiHttpClient.GetPagesBatchAsync(
                    pagesBatchRequest: request,
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    versionDescriptor: versionDescriptor,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>.Success(pages, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<string>> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                Stream stream = await _wikiHttpClient.GetPageTextAsync(
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: path,
                    recursionLevel: VersionControlRecursionType.None,
                    versionDescriptor: null,
                    includeContent: true,
                    cancellationToken: cancellationToken);

                using StreamReader reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync(cancellationToken);
                return AzureDevOpsActionResult<string>.Success(content, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<WikiPageResponse>> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPageResponse response = await _wikiHttpClient.DeletePageAsync(project: _projectName, wikiIdentifier: wikiId, path: path, versionDescriptor: gitVersionDescriptor, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiPageResponse>.Success(response, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiPageResponse>.Failure(ex, _logger);
            }
        }
    }
}