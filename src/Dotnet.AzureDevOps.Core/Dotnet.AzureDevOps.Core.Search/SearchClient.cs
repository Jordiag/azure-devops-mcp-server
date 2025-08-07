using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Search.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dotnet.AzureDevOps.Core.Search;

public class SearchClient : ISearchClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;

    public SearchClient(string searchOrganizationUrl, string personalAccessToken, ILogger? logger = null)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(searchOrganizationUrl) };
        string token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _logger = logger ?? NullLogger.Instance;
    }

    public Task<AzureDevOpsActionResult<string>> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default)
    {
        string? projectName = options.Project?[0];
        return projectName == null
            ? Task.FromResult(AzureDevOpsActionResult<string>.Failure("Project name must be specified in CodeSearchOptions.", _logger))
            : SendSearchRequestAsync($"{projectName}/_apis/search/codesearchresults", BuildCodePayload(options), cancellationToken);
    }

    /// <summary>
    /// Searches for wiki pages using the provided options.
    /// Returns a JSON string with the search results.
    /// </summary>
    /// <param name="options">Wiki search options including search text, project, wiki, etc.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>AzureDevOpsActionResult containing the search result JSON string or error details.</returns>
    public Task<AzureDevOpsActionResult<string>> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("_apis/search/wikisearchresults", BuildWikiPayload(options), cancellationToken);

    /// <summary>
    /// Searches for work items using the provided options.
    /// Returns a JSON string with the search results.
    /// </summary>
    /// <param name="options">Work item search options including search text, project, area path, etc.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>AzureDevOpsActionResult containing the search result JSON string or error details.</returns>
    public Task<AzureDevOpsActionResult<string>> SearchWorkItemsAsync(WorkItemSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("_apis/search/workitemsearchresults", BuildWorkItemPayload(options), cancellationToken);

    /// <summary>
    /// Checks if the Azure DevOps Code Search extension is enabled for the organization.
    /// Makes a GET request to the extension management API.
    /// Returns true if enabled, false if not found, or error details if the request fails.
    /// Peculiarity: If the extension is not found (404), returns Success(false).
    /// If the extension is found, inspects the "installState.flags" property for "trusted" to determine if enabled.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>AzureDevOpsActionResult containing a boolean indicating if code search is enabled, or error details.</returns>
    public async Task<AzureDevOpsActionResult<bool>> IsCodeSearchEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string url = "_apis/extensionmanagement/installedextensionsbyname/ms/vss-code-search?api-version=7.1";

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return AzureDevOpsActionResult<bool>.Success(false, _logger);
            }

            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement extension = JsonSerializer.Deserialize<JsonElement>(content);

            if(extension.TryGetProperty("installState", out JsonElement installState))
            {
                bool enabled = installState.TryGetProperty("flags", out JsonElement flags) &&
                               flags.GetString()?.Contains("trusted") == true;
                return AzureDevOpsActionResult<bool>.Success(enabled, _logger);
            }

            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
        }
    }

    /// <summary>
    /// Sends a POST request to the Azure DevOps Search API with the specified resource and payload.
    /// Returns the raw JSON response as a string.
    /// If the response is not successful, returns a failed result with the status code and error message.
    /// </summary>
    /// <param name="resource">API resource path (e.g., codesearchresults).</param>
    /// <param name="payload">Request payload object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>AzureDevOpsActionResult containing the response JSON string or error details.</returns>
    private async Task<AzureDevOpsActionResult<string>> SendSearchRequestAsync(string resource, object payload, CancellationToken cancellationToken)
    {
        string url = $"{resource}?api-version={GlobalConstants.ApiVersion}";
        try
        {
            using HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, url, payload, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<string>.Failure(response.StatusCode, error, _logger);
            }
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return AzureDevOpsActionResult<string>.Success(content, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<string>.Failure(ex, _logger);
        }
    }

    /// <summary>
    /// Builds the payload for a code search request.
    /// Includes filters for project, repository, path, and branch if specified.
    /// </summary>
    /// <param name="options">Code search options.</param>
    /// <returns>Dictionary representing the request payload.</returns>
    private static Dictionary<string, object?> BuildCodePayload(CodeSearchOptions options)
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

    /// <summary>
    /// Builds the payload for a wiki search request.
    /// Includes filters for project and wiki if specified.
    /// </summary>
    /// <param name="options">Wiki search options.</param>
    /// <returns>WikiSearchPayload object for the request.</returns>
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

    /// <summary>
    /// Builds the payload for a work item search request.
    /// Includes filters for project, area path, work item type, state, and assigned to if specified.
    /// </summary>
    /// <param name="options">Work item search options.</param>
    /// <returns>Dictionary representing the request payload.</returns>
    private static Dictionary<string, object?> BuildWorkItemPayload(WorkItemSearchOptions options)
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