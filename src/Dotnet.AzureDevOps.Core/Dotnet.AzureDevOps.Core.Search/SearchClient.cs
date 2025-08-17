using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Dotnet.AzureDevOps.Core.Common.Services;
using Dotnet.AzureDevOps.Core.Search.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dotnet.AzureDevOps.Core.Search;

public class SearchClient : AzureDevOpsClientBase, ISearchClient
{
    private readonly HttpClient _httpClient;

    public SearchClient(HttpClient httpClient, ILogger<SearchClient>? logger = null, IRetryService? retryService = null, IExceptionHandlingService? exceptionHandlingService = null)
        : base("https://dev.azure.com/placeholder", "placeholder-token", "placeholder-project", logger, retryService, exceptionHandlingService)
    {
        _httpClient = httpClient;
    }

    public SearchClient(string organizationUrl, string personalAccessToken, HttpClient httpClient, ILogger<SearchClient>? logger = null, IRetryService? retryService = null, IExceptionHandlingService? exceptionHandlingService = null)
        : base(organizationUrl, personalAccessToken, "placeholder-project", logger, retryService, exceptionHandlingService)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Searches for code content across repositories within the specified Azure DevOps project using the Code Search extension.
    /// Performs full-text search through source code files, supporting advanced filtering by repository, file path, and branch.
    /// Requires the Azure DevOps Code Search extension to be installed and enabled for the organization.
    /// Returns raw JSON results containing file matches, line numbers, and preview snippets of matching code.
    /// </summary>
    /// <param name="options">Code search configuration including search text, project filters, repository filters, path filters, branch filters, and pagination options. Project name is required.</param>
    /// <param name="cancellationToken">Optional token to cancel the search operation if needed.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: JSON string with code search results including file paths, line numbers, code snippets, and faceted navigation data
    /// - Failure: Error details if project name is missing, search request fails, or Code Search extension is not available
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no project is specified in the search options</exception>
    /// <exception cref="HttpRequestException">Thrown when the Azure DevOps API request fails due to network issues or invalid credentials</exception>
    /// <exception cref="JsonException">Thrown when the API response cannot be parsed as valid JSON</exception>
    public Task<AzureDevOpsActionResult<string>> SearchCodeAsync(CodeSearchOptions options, CancellationToken cancellationToken = default)
    {
        string? projectName = options.Project?[0];
        return projectName == null
            ? Task.FromResult(AzureDevOpsActionResult<string>.Failure("Project name must be specified in CodeSearchOptions.", Logger))
            : SendSearchRequestAsync($"{projectName}/_apis/search/codesearchresults", BuildCodePayload(options), cancellationToken);
    }

    /// <summary>
    /// Searches for content across wiki pages within the Azure DevOps organization using full-text search capabilities.
    /// Performs comprehensive search through wiki page titles, content, and metadata across all accessible wikis.
    /// Supports filtering by specific projects and wiki repositories to narrow search scope and improve relevance.
    /// Returns structured JSON results with matching wiki pages, content previews, and hierarchical navigation data.
    /// Enables knowledge discovery and content location across distributed documentation and collaborative wiki spaces.
    /// </summary>
    /// <param name="options">Wiki search configuration including search text, project filters, wiki repository filters, and result pagination options for controlling scope and output volume.</param>
    /// <param name="cancellationToken">Optional token to cancel the search operation if the request needs to be terminated early.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: JSON string with wiki search results including page titles, content excerpts, wiki hierarchy, modification dates, and faceted filtering options
    /// - Failure: Error details if search request fails, authentication issues occur, or wiki search service is unavailable
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the Azure DevOps API request fails due to network connectivity issues or service unavailability</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails or insufficient permissions exist to access wiki content</exception>
    /// <exception cref="JsonException">Thrown when the search response contains invalid JSON or unexpected data structure</exception>
    public Task<AzureDevOpsActionResult<string>> SearchWikiAsync(WikiSearchOptions options, CancellationToken cancellationToken = default)
        => SendSearchRequestAsync("_apis/search/wikisearchresults", BuildWikiPayload(options), cancellationToken);

    /// <summary>
    /// Searches for work items across Azure DevOps projects using advanced full-text search and sophisticated filtering capabilities.
    /// Performs comprehensive search through work item titles, descriptions, comments, and custom field values to locate relevant items.
    /// Supports complex filtering by project, area path, work item type, state, assignee, and other system and custom fields.
    /// Enables rapid work item discovery, progress tracking, and project management through powerful search and faceted navigation.
    /// Returns detailed results with work item metadata, field values, and relationship information for comprehensive project visibility.
    /// </summary>
    /// <param name="options">Work item search configuration including search text, project scope, area path filters, work item type restrictions, state filters, assignee filters, and pagination controls for precise result management.</param>
    /// <param name="cancellationToken">Optional token to cancel the search operation if the request needs to be terminated before completion.</param>
    /// <returns>
    /// Task resolving to AzureDevOpsActionResult containing:
    /// - Success: JSON string with work item search results including IDs, titles, descriptions, field values, state information, assignment details, and faceted filtering data
    /// - Failure: Error details if search query is malformed, authentication fails, insufficient permissions exist, or work item search service encounters issues
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the Azure DevOps API request fails due to network connectivity problems or service interruption</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authentication credentials are invalid or insufficient permissions exist to access work item data</exception>
    /// <exception cref="JsonException">Thrown when the search response contains malformed JSON or unexpected response structure from the search service</exception>
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
            bool enabled = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                string url = "_apis/extensionmanagement/installedextensionsbyname/ms/vss-code-search?api-version=7.1";

                using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new AzureDevOpsApiException(
                        $"Failed to check code search extension status: {response.StatusCode} - {response.ReasonPhrase}",
                        (int)response.StatusCode,
                        error,
                        "IsCodeSearchEnabled");
                }

                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                JsonElement extension = JsonSerializer.Deserialize<JsonElement>(content);

                if (extension.TryGetProperty("installState", out JsonElement installState))
                {
                    return installState.TryGetProperty("flags", out JsonElement flags) &&
                           flags.GetString()?.Contains("trusted") == true;
                }

                return true;
            }, "IsCodeSearchEnabled", OperationType.Read);

            return AzureDevOpsActionResult<bool>.Success(enabled, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
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
        string correlationId = Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            Logger.LogDebug("Starting search request to {Resource}. CorrelationId: {CorrelationId}", resource, correlationId);

            string content = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, url, payload, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            $"Authentication failed for search request to {resource}",
                            $"Search-{resource}",
                            correlationId),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            $"Access denied for search request to {resource}",
                            $"Search-{resource}",
                            correlationId),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Search endpoint not found: {resource}",
                            "SearchEndpoint",
                            resource,
                            $"Search-{resource}",
                            correlationId),
                        _ => new AzureDevOpsApiException(
                            $"Search request failed for {resource}: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            $"Search-{resource}",
                            correlationId)
                    };

                    throw exception;
                }
                
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }, $"Search-{resource}", OperationType.Read, correlationId);
            
            Logger.LogDebug("Search request completed successfully for {Resource}. CorrelationId: {CorrelationId}", resource, correlationId);
            return AzureDevOpsActionResult<string>.Success(content, Logger);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<string>.Failure(ex, Logger);
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