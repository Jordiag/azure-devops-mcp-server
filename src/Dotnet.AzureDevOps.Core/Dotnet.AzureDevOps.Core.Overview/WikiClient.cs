using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Dotnet.AzureDevOps.Core.Common;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class WikiClient : IWikiClient
    {
        private readonly string _projectName;
        private readonly WikiHttpClient _wikiHttpClient;
        private readonly ProjectHttpClient _projectHttpClient;
        private readonly DashboardHttpClient _dashboardHttpClient;
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
            _projectHttpClient = connection.GetClient<ProjectHttpClient>();
            _dashboardHttpClient = connection.GetClient<DashboardHttpClient>();
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

        public Task DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default) => _wikiHttpClient.DeleteWikiAsync(wikiIdentifier: wikiId, cancellationToken: cancellationToken);

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

                using StreamReader reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                return content;
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        public async Task<string> SearchWikiAsync(WikiSearchOptions searchOptions, CancellationToken cancellationToken = default)
        {
            using HttpClient httpClient = new HttpClient { BaseAddress = new Uri(_organizationUrl) };
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            Dictionary<string, string[]> filters = new Dictionary<string, string[]>();
            if (searchOptions.Project is { Count: > 0 })
            {
                filters["Project"] = searchOptions.Project.ToArray();
            }
            if (searchOptions.Wiki is { Count: > 0 })
            {
                filters["Wiki"] = searchOptions.Wiki.ToArray();
            }

            var body = new Dictionary<string, object?>
            {
                ["searchText"] = searchOptions.SearchText,
                ["includeFacets"] = searchOptions.IncludeFacets,
                ["$skip"] = searchOptions.Skip,
                ["$top"] = searchOptions.Top
            };
            if (filters.Count > 0)
            {
                body["filters"] = filters;
            }

            using HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
                httpClient,
                requestUri: $"_apis/search/wikisearchresults?api-version={Constants.ApiVersion}",
                value: body,
                cancellationToken: cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }


        public Task DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default) =>
            _wikiHttpClient.DeletePageAsync(project: _projectName, wikiIdentifier: wikiId, path: path, versionDescriptor: gitVersionDescriptor, cancellationToken: cancellationToken);
    }
}
