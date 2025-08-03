using System.Text;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview.Options;
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
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;

        public WikiClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _projectName = projectName;
            _organizationUrl = organizationUrl;
            _personalAccessToken = personalAccessToken;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _wikiHttpClient = connection.GetClient<WikiHttpClient>();
        }

        public async Task<Guid> CreateWikiAsync(WikiCreateOptions wikiCreateOptions, CancellationToken cancellationToken = default)
        {
            var wikiCreateParameters = new WikiCreateParametersV2
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
            return wiki.Id;
        }

        public async Task<WikiV2?> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _wikiHttpClient.GetWikiAsync(project: _projectName, wikiIdentifier: wikiId, cancellationToken: cancellationToken);
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<WikiV2>> ListWikisAsync(CancellationToken cancellationToken = default)
        {
            List<WikiV2> wikis = await _wikiHttpClient.GetAllWikisAsync(project: _projectName, cancellationToken: cancellationToken);

            return wikis;
        }

        public Task<WikiV2> DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default) => _wikiHttpClient.DeleteWikiAsync(wikiIdentifier: wikiId, cancellationToken: cancellationToken);

        public async Task<int?> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions wikiPageUpdateOptions, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default)
        {
            var pageParameters = new WikiPageCreateOrUpdateParameters
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
                cancellationToken: cancellationToken
                );

            return response.Page?.Id;
        }

        public async Task<WikiPageResponse?> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _wikiHttpClient.GetPageAsync(
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: path,
                    includeContent: true,
                    cancellationToken: cancellationToken);
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<WikiPageDetail>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions pagesOptions, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default)
        {
            var request = new WikiPagesBatchRequest
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

            return pages;
        }

        public async Task<string?> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
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

                using var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync(cancellationToken);
                return content;
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        public Task<WikiPageResponse> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default) => 
            _wikiHttpClient.DeletePageAsync(project: _projectName, wikiIdentifier: wikiId, path: path, versionDescriptor: gitVersionDescriptor, cancellationToken: cancellationToken);
    }
}
