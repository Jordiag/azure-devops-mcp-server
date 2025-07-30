using System.Net.Http.Headers;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Search.Options;

namespace Dotnet.AzureDevOps.Core.Search;

public class SearchClient : ISearchClient
{
    private readonly HttpClient _httpClient;

    public SearchClient(string organizationUrl, string personalAccessToken)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };
        string token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public Task<string> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("codesearchresults", BuildCodePayload(options), cancellationToken);

    public Task<string> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("wikisearchresults", BuildWikiPayload(options), cancellationToken);

    public Task<string> SearchWorkItemsAsync(WorkItemSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("workitemsearchresults", BuildWorkItemPayload(options), cancellationToken);

    private async Task<string> SendSearchRequestAsync(string resource, object payload, CancellationToken cancellationToken)
    {
        string url = $"_apis/search/{resource}?api-version={GlobalConstants.ApiVersion}";
        using HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
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

    private static object BuildWikiPayload(WikiSearchOptions options)
    {
        var filters = new Dictionary<string, IReadOnlyList<string>>();
        if(options.Project != null && options.Project.Count > 0)
            filters["Project"] = options.Project;
        if(options.Wiki != null && options.Wiki.Count > 0)
            filters["Wiki"] = options.Wiki;

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
