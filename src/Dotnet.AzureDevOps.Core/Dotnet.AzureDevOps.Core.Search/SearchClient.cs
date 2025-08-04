using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Search.Options;

namespace Dotnet.AzureDevOps.Core.Search;

public class SearchClient : ISearchClient
{
    private readonly HttpClient _httpClient;

    public SearchClient(string organisation, string personalAccessToken)
    {
        string searchBaseAddress = $"https://almsearch.dev.azure.com/{organisation}/";
        _httpClient = new HttpClient { BaseAddress = new Uri(searchBaseAddress) };
        string token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public Task<AzureDevOpsActionResult<string>> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default)
    {
        string? projectName = options.Project?[0];
        return projectName == null
            ? Task.FromResult(AzureDevOpsActionResult<string>.Failure("Project name must be specified in CodeSearchOptions."))
            : SendSearchRequestAsync($"{projectName}/_apis/search/codesearchresults", BuildCodePayload(options), cancellationToken);
    }

    public Task<AzureDevOpsActionResult<string>> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("_apis/search/wikisearchresults", BuildWikiPayload(options), cancellationToken);

    public Task<AzureDevOpsActionResult<string>> SearchWorkItemsAsync(WorkItemSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("_apis/search/workitemsearchresults", BuildWorkItemPayload(options), cancellationToken);

    public async Task<AzureDevOpsActionResult<bool>> IsCodeSearchEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string url = "_apis/extensionmanagement/installedextensionsbyname/ms/vss-code-search?api-version=7.1";

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return AzureDevOpsActionResult<bool>.Success(false);
            }

            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement extension = JsonSerializer.Deserialize<JsonElement>(content);

            if(extension.TryGetProperty("installState", out JsonElement installState))
            {
                bool enabled = installState.TryGetProperty("flags", out JsonElement flags) &&
                               flags.GetString()?.Contains("trusted") == true;
                return AzureDevOpsActionResult<bool>.Success(enabled);
            }

            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    private async Task<AzureDevOpsActionResult<string>> SendSearchRequestAsync(string resource, object payload, CancellationToken cancellationToken)
    {
        string url = $"{resource}?api-version={GlobalConstants.ApiVersion}";
        try
        {
            using HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, url, payload, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<string>.Failure(response.StatusCode, error);
            }
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return AzureDevOpsActionResult<string>.Success(content);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<string>.Failure(ex);
        }
    }

    private static object BuildCodePayload(CodeSearchOptions options)
    {
        var filters = new Dictionary<string, IReadOnlyList<string>>();
        if(options.Project != null && options.Project.Count > 0)
            filters["Project"] = options.Project;
        if(options.Repository != null && options.Repository.Count > 0)
            filters["Repository"] = options.Repository;
        if(options.Path != null && options.Path.Count > 0)
            filters["Path"] = options.Path;
        if(options.Branch != null && options.Branch.Count > 0)
            filters["Branch"] = options.Branch;

        var payload = new Dictionary<string, object?>
        {
            ["searchText"] = options.SearchText,
            ["includeFacets"] = options.IncludeFacets,
            ["$skip"] = options.Skip,
            ["$top"] = options.Top
        };

        if(filters.Count > 0)
            payload["filters"] = filters;

        return payload;
    }

    private static WikiSearchPayload BuildWikiPayload(WikiSearchOptions options)
    {
        var filters = new Dictionary<string, IReadOnlyList<string>>();

        if(options.Project?.Count > 0)
            filters["Project"] = options.Project;

        if(options.Wiki?.Count > 0)
            filters["Wiki"] = options.Wiki;

        return new WikiSearchPayload
        {
            SearchText = options.SearchText,
            IncludeFacets = options.IncludeFacets,
            Skip = options.Skip,
            Top = options.Top,
            Filters = filters.Count > 0 ? filters : null
        };
    }

    private static object BuildWorkItemPayload(WorkItemSearchOptions options)
    {
        var filters = new Dictionary<string, IReadOnlyList<string>>();
        if(options.Project != null && options.Project.Count > 0)
            filters["System.TeamProject"] = options.Project;
        if(options.AreaPath != null && options.AreaPath.Count > 0)
            filters["System.AreaPath"] = options.AreaPath;
        if(options.WorkItemType != null && options.WorkItemType.Count > 0)
            filters["System.WorkItemType"] = options.WorkItemType;
        if(options.State != null && options.State.Count > 0)
            filters["System.State"] = options.State;
        if(options.AssignedTo != null && options.AssignedTo.Count > 0)
            filters["System.AssignedTo"] = options.AssignedTo;

        var payload = new Dictionary<string, object?>
        {
            ["searchText"] = options.SearchText,
            ["includeFacets"] = options.IncludeFacets,
            ["$skip"] = options.Skip,
            ["$top"] = options.Top
        };
        if(filters.Count > 0)
            payload["filters"] = filters;
        return payload;
    }
}
