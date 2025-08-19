using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : AzureDevOpsClientBase, IArtifactsClient
{
    private const string ApiVersion = GlobalConstants.ApiVersion;
    
    // Operation constants
    private const string CreateFeedOperation = "CreateFeed";
    private const string UpdateFeedOperation = "UpdateFeed";
    private const string GetFeedOperation = "GetFeed";
    private const string ListFeedsOperation = "ListFeeds";
    private const string DeleteFeedOperation = "DeleteFeed";
    private const string ListPackagesOperation = "ListPackages";
    private const string DeletePackageOperation = "DeletePackage";
    private const string GetFeedPermissionsOperation = "GetFeedPermissions";
    private const string CreateFeedViewOperation = "CreateFeedView";
    private const string ListFeedViewsOperation = "ListFeedViews";
    private const string DeleteFeedViewOperation = "DeleteFeedView";
    private const string SetUpstreamingBehaviorOperation = "SetUpstreamingBehavior";
    private const string GetUpstreamingBehaviorOperation = "GetUpstreamingBehavior";
    private const string GetPackageVersionOperation = "GetPackageVersion";
    private const string UpdatePackageVersionOperation = "UpdatePackageVersion";
    private const string DownloadPackageOperation = "DownloadPackage";
    private const string GetRetentionPolicyOperation = "GetRetentionPolicy";
    private const string SetRetentionPolicyOperation = "SetRetentionPolicy";
    
    // Resource type constants
    private const string FeedResourceType = "Feed";
    private const string PackageResourceType = "Package";
    private const string FeedViewResourceType = "FeedView";

    private readonly string _projectName;
    private readonly HttpClient _httpClient;
    private readonly string _organizationUrl;

    public ArtifactsClient(HttpClient httpClient, string projectName, ILogger<ArtifactsClient>? logger = null, IRetryService? retryService = null, IExceptionHandlingService? exceptionHandlingService = null)
        : base(httpClient.BaseAddress?.ToString()?.Replace("https://feeds.dev.azure.com", "https://dev.azure.com") ?? "", "placeholder-token", projectName, logger, retryService, exceptionHandlingService)
    {
        _projectName = projectName.TrimEnd('/');
        _httpClient = httpClient;
        _organizationUrl = httpClient.BaseAddress?.ToString()?.Replace("https://dev.azure.com", "https://feeds.dev.azure.com") ?? "";
    }

    public ArtifactsClient(string organizationUrl, string personalAccessToken, string projectName, HttpClient httpClient, ILogger<ArtifactsClient>? logger = null, IRetryService? retryService = null, IExceptionHandlingService? exceptionHandlingService = null)
        : base(organizationUrl, personalAccessToken, projectName, logger, retryService, exceptionHandlingService)
    {
        _projectName = projectName.TrimEnd('/');
        _httpClient = httpClient;
        _organizationUrl = httpClient.BaseAddress?.ToString()?.Replace("https://dev.azure.com", "https://feeds.dev.azure.com") ?? "";
    }

    /// <summary>
    /// Creates a new package feed within the Azure DevOps project.
    /// Package feeds are containers for packages such as NuGet, npm, Maven, and Python packages.
    /// A feed provides a central location for teams to store, manage, and share packages.
    /// </summary>
    /// <param name="feedCreateOptions">Configuration options for the new feed including name and description.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique identifier (GUID) of the created feed if successful,
    /// or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedCreateOptions contains invalid data.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<AzureDevOpsActionResult<Guid>> CreateFeedAsync(FeedCreateOptions feedCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            Guid feedId = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                string feedsUrl = $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}";
                object payload = new { name = feedCreateOptions.Name, description = feedCreateOptions.Description };
                using HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, feedsUrl, payload, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while creating feed",
                            CreateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while creating feed",
                            CreateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Conflict => new AzureDevOpsApiException(
                            $"Feed with name '{feedCreateOptions.Name}' already exists",
                            (int)response.StatusCode,
                            error,
                            CreateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to create feed: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            CreateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
                return feed!.Id;
            }, CreateFeedOperation, OperationType.Create);

            return AzureDevOpsActionResult<Guid>.Success(feedId, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Guid>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Updates an existing package feed's properties such as name and description.
    /// This method allows you to modify feed metadata without affecting the packages stored within the feed.
    /// Only the specified properties in the update options will be changed.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed to update.</param>
    /// <param name="feedUpdateOptions">The properties to update, containing new name and/or description values.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the update was successful,
    /// or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty or feedUpdateOptions is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<AzureDevOpsActionResult<bool>> UpdateFeedAsync(Guid feedId, FeedUpdateOptions feedUpdateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                Dictionary<string, string?> fields = [];
                if(feedUpdateOptions.Name is { Length: > 0 })
                {
                    fields["name"] = feedUpdateOptions.Name;
                }
                if(feedUpdateOptions.Description is { Length: > 0 })
                {
                    fields["description"] = feedUpdateOptions.Description;
                }
                if(fields.Count == 0)
                {
                    return true;
                }

                using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}")
                {
                    Content = JsonContent.Create(fields)
                };
                using HttpResponseMessage response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while updating feed",
                            UpdateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while updating feed",
                            UpdateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            UpdateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to update feed: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            UpdateFeedOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, UpdateFeedOperation, OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>. Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific package feed.
    /// This includes feed metadata such as name, description, URL, capabilities, and creation date.
    /// Use this method to get comprehensive information about a feed before performing operations on it.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="Feed"/> object with complete feed information if successful,
    /// or error details if the operation fails or the feed is not found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<AzureDevOpsActionResult<Feed>> GetFeedAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            Feed feed = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while retrieving feed",
                            GetFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while retrieving feed",
                            GetFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            GetFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to retrieve feed: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            GetFeedOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
                return feed!;
            }, GetFeedOperation, OperationType.Read);

            return AzureDevOpsActionResult<Feed>.Success(feed, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Feed>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a list of all package feeds available in the Azure DevOps project.
    /// This method provides an overview of all feeds that the user has access to, including their basic metadata.
    /// Use this to discover available feeds before performing specific operations.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of <see cref="Feed"/> objects if successful,
    /// or an empty list if no feeds exist, or error details if the operation fails.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<Feed>>> ListFeedsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<Feed> feeds = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while listing feeds",
                            ListFeedsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while listing feeds",
                            ListFeedsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to list feeds: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            ListFeedsOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedList? list = await response.Content.ReadFromJsonAsync<FeedList>(cancellationToken);
                return list?.Value?.ToArray() ?? Array.Empty<Feed>();
            }, ListFeedsOperation, OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Success(feeds, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Permanently deletes a package feed and all its contents from the Azure DevOps project.
    /// This operation is irreversible and will remove all packages, versions, and metadata associated with the feed.
    /// Use with extreme caution as this will impact all users and systems depending on the feed.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed to delete.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the deletion was successful,
    /// or error details if the operation fails or the feed is not found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete the feed.</exception>
    public async Task<AzureDevOpsActionResult<bool>> DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while deleting feed",
                            DeleteFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while deleting feed",
                            DeleteFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            DeleteFeedOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to delete feed: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            DeleteFeedOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, DeleteFeedOperation, OperationType.Delete);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves a list of all packages contained within a specific feed.
    /// This includes all package types (NuGet, npm, Maven, Python, etc.) and their basic metadata such as name, version, and last modified date.
    /// Use this method to get an overview of all packages in a feed for management or reporting purposes.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed whose packages to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of <see cref="Package"/> objects if successful,
    /// or an empty list if the feed contains no packages, or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the feed.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<Package>>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<Package> packages = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while listing packages",
                            ListPackagesOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while listing packages",
                            ListPackagesOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            ListPackagesOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to list packages: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            ListPackagesOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                PackageList? list = await response.Content.ReadFromJsonAsync<PackageList>(cancellationToken);
                return list?.Value?.ToArray() ?? Array.Empty<Package>();
            }, ListPackagesOperation, OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Success(packages, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Permanently deletes a specific version of a package from the feed.
    /// This operation is irreversible and will remove the package version and its associated metadata.
    /// Other versions of the same package will remain intact. Use with caution as this may break dependencies.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package to delete.</param>
    /// <param name="version">The specific version of the package to delete (e.g., "1.0.0").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the deletion was successful,
    /// or error details if the operation fails, the package is not found, or deletion is not allowed.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, or packageName/version is null or empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete packages from the feed.</exception>
    public async Task<AzureDevOpsActionResult<bool>> DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages/{packageName}/versions/{version}?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while deleting package",
                            DeletePackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while deleting package",
                            DeletePackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' version '{version}' not found in feed",
                            PackageResourceType,
                            $"{packageName}@{version}",
                            DeletePackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to delete package: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            DeletePackageOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, DeletePackageOperation, OperationType.Delete);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves the access permissions for a specific feed, showing who has access and what level of permissions they have.
    /// This includes user and group permissions such as Reader, Contributor, Collaborator, and Owner roles.
    /// Use this method to audit feed security or before modifying permissions.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed whose permissions to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of <see cref="FeedPermission"/> objects if successful,
    /// or an empty list if no explicit permissions are set, or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view feed permissions.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<FeedPermission> permissions = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/permissions?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while retrieving feed permissions",
                            GetFeedPermissionsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while retrieving feed permissions",
                            GetFeedPermissionsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            GetFeedPermissionsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to retrieve feed permissions: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            GetFeedPermissionsOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedPermissionList? list = await response.Content.ReadFromJsonAsync<FeedPermissionList>(options, cancellationToken);
                return list?.Value?.ToArray() ?? Array.Empty<FeedPermission>();
            }, GetFeedPermissionsOperation, OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Success(permissions, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Creates a new view for a package feed, which allows filtering and organizing packages within the feed.
    /// Views can be used to create different perspectives on the same feed, such as showing only released packages
    /// or packages for specific audiences. Views help organize package consumption and access control.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed in which to create the view.</param>
    /// <param name="feedView">The view configuration including name, type, and filtering criteria.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the created <see cref="FeedView"/> object if successful,
    /// or error details if the operation fails or if a view with the same name already exists.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty or feedView is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create views in the feed.</exception>
    public async Task<AzureDevOpsActionResult<FeedView>> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default)
    {
        try
        {
            FeedView created = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                using HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}", feedView, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while creating feed view",
                            CreateFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while creating feed view",
                            CreateFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            CreateFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Conflict => new AzureDevOpsApiException(
                            $"Feed view with name '{feedView.Name}' already exists",
                            (int)response.StatusCode,
                            error,
                            CreateFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to create feed view: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            CreateFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedView? createdView = await response.Content.ReadFromJsonAsync<FeedView>(options, cancellationToken);
                return createdView!;
            }, CreateFeedViewOperation, OperationType.Create);

            return AzureDevOpsActionResult<FeedView>.Success(created, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedView>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves all views configured for a specific feed.
    /// Views provide different perspectives on the same feed content, such as release views, prerelease views,
    /// or audience-specific views. This method helps discover available views and their configurations.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed whose views to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of <see cref="FeedView"/> objects if successful,
    /// including at least the default "@local" view, or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the feed.</exception>
    public async Task<AzureDevOpsActionResult<IReadOnlyList<FeedView>>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<FeedView> views = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while listing feed views",
                            ListFeedViewsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while listing feed views",
                            ListFeedViewsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            ListFeedViewsOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to list feed views: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            ListFeedViewsOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedViewList? list = await response.Content.ReadFromJsonAsync<FeedViewList>(cancellationToken);
                return list?.Value?.ToArray() ?? Array.Empty<FeedView>();
            }, ListFeedViewsOperation, OperationType.Read);

            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Success(views, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Permanently deletes a feed view and its configuration.
    /// This operation removes the view and any custom filtering or access rules associated with it.
    /// The default "@local" view cannot be deleted. Use with caution as this may affect package consumers using the view.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the view.</param>
    /// <param name="viewId">The identifier of the view to delete (e.g., "@release", "@prerelease").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the deletion was successful,
    /// or error details if the operation fails, the view is not found, or deletion is not allowed (e.g., default view).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty or viewId is null or empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete views from the feed.</exception>
    public async Task<AzureDevOpsActionResult<bool>> DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views/{viewId}?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while deleting feed view",
                            DeleteFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while deleting feed view",
                            DeleteFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed view '{viewId}' not found in feed '{feedId}'",
                            FeedViewResourceType,
                            viewId,
                            DeleteFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.BadRequest => new AzureDevOpsApiException(
                            $"Cannot delete default view '{viewId}'",
                            (int)response.StatusCode,
                            error,
                            DeleteFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to delete feed view: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            DeleteFeedViewOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, DeleteFeedViewOperation, OperationType.Delete);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Configures the upstreaming behavior for a specific package in the feed.
    /// Upstreaming behavior determines how the feed handles requests for packages that may exist in upstream sources
    /// (like nuget.org). Options include allowing upstreaming, blocking it, or requiring local copies only.
    /// This helps control package consumption and security policies.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package for which to set upstreaming behavior.</param>
    /// <param name="behavior">The upstreaming behavior policy to apply (Allow, Block, etc.).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the behavior was set successfully,
    /// or error details if the operation fails or the package is not found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName is null/empty, or behavior is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify feed settings.</exception>
    public async Task<AzureDevOpsActionResult<bool>> SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}", behavior, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while setting upstreaming behavior",
                            SetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while setting upstreaming behavior",
                            SetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' not found in feed '{feedId}'",
                            PackageResourceType,
                            packageName,
                            SetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to set upstreaming behavior: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            SetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, SetUpstreamingBehaviorOperation, OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves the current upstreaming behavior configuration for a specific package.
    /// This shows how the feed handles requests for the package when it might be available from upstream sources.
    /// Use this method to check current policies before modifying them or for auditing purposes.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package whose upstreaming behavior to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="UpstreamingBehavior"/> configuration if successful,
    /// or error details if the operation fails or the package is not found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty or packageName is null or empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access feed settings.</exception>
    public async Task<AzureDevOpsActionResult<UpstreamingBehavior>> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            UpstreamingBehavior behavior = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while retrieving upstreaming behavior",
                            GetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while retrieving upstreaming behavior",
                            GetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' not found in feed '{feedId}'",
                            PackageResourceType,
                            packageName,
                            GetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to retrieve upstreaming behavior: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            GetUpstreamingBehaviorOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                UpstreamingBehavior result = await response.Content.ReadFromJsonAsync<UpstreamingBehavior>(cancellationToken);
                return result;
            }, GetUpstreamingBehaviorOperation, OperationType.Read);

            return AzureDevOpsActionResult<UpstreamingBehavior>.Success(behavior, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<UpstreamingBehavior>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific version of a package in the feed.
    /// This includes package metadata such as description, dependencies, download count, publish date,
    /// and version-specific information. Use this method to get comprehensive details about a package version.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package to retrieve information for.</param>
    /// <param name="version">The specific version of the package to retrieve (e.g., "1.2.3").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="Package"/> object with detailed version information if successful,
    /// or error details if the operation fails, the package is not found, or the version doesn't exist.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName/version is null or empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the feed or package.</exception>
    public async Task<AzureDevOpsActionResult<Package>> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            Package package = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while retrieving package version",
                            GetPackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while retrieving package version",
                            GetPackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' version '{version}' not found in feed",
                            PackageResourceType,
                            $"{packageName}@{version}",
                            GetPackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to retrieve package version: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            GetPackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                Package? pkg = await response.Content.ReadFromJsonAsync<Package>(cancellationToken);
                return pkg!;
            }, GetPackageVersionOperation, OperationType.Read);

            return AzureDevOpsActionResult<Package>.Success(package, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Package>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Updates the metadata and properties of a specific package version in the feed.
    /// This allows modifying package details such as listing status (listed/unlisted), quality level,
    /// or other version-specific metadata without republishing the package.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package whose version to update.</param>
    /// <param name="version">The specific version of the package to update (e.g., "1.2.3").</param>
    /// <param name="details">The updated package version details and metadata.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the update was successful,
    /// or error details if the operation fails, the package is not found, or the version doesn't exist.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName/version is null/empty, or details is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify packages in the feed.</exception>
    public async Task<AzureDevOpsActionResult<bool>> UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}")
                {
                    Content = JsonContent.Create(details)
                };
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while updating package version",
                            UpdatePackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while updating package version",
                            UpdatePackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' version '{version}' not found in feed",
                            PackageResourceType,
                            $"{packageName}@{version}",
                            UpdatePackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to update package version: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            UpdatePackageVersionOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                return true;
            }, UpdatePackageVersionOperation, OperationType.Update);

            return AzureDevOpsActionResult<bool>.Success(result, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Downloads the content of a specific package version as a stream.
    /// This provides access to the actual package file (e.g., .nupkg for NuGet, .tgz for npm) that can be
    /// saved to disk, processed, or streamed to clients. The returned stream should be properly disposed of after use.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed containing the package.</param>
    /// <param name="packageName">The name of the package to download.</param>
    /// <param name="version">The specific version of the package to download (e.g., "1.2.3").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing a <see cref="Stream"/> with the package content if successful,
    /// or error details if the operation fails, the package is not found, or the version doesn't exist.
    /// The caller is responsible to disposing of the returned stream.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName is null/empty, or version is null/empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to download from the feed.</exception>
    public async Task<AzureDevOpsActionResult<Stream>> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            Stream contentStream = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}/content?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while downloading package",
                            DownloadPackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while downloading package",
                            DownloadPackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Package '{packageName}' version '{version}' not found in feed",
                            PackageResourceType,
                            $"{packageName}@{version}",
                            DownloadPackageOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to download package: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            DownloadPackageOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return stream;
            }, DownloadPackageOperation, OperationType.Read);

            return AzureDevOpsActionResult<Stream>.Success(contentStream, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Stream>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Retrieves the retention policy configuration for a specific feed.
    /// Retention policies automatically manage package lifecycle by deleting old versions based on age,
    /// count limits, or other criteria. This helps manage storage costs and maintain feed cleanliness.
    /// Returns null if no retention policy is configured for the feed.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed whose retention policy to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the <see cref="FeedRetentionPolicy"/> if a policy exists,
    /// or a NotFound result if no retention policy is configured, or error details if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access feed settings.</exception>
    public async Task<AzureDevOpsActionResult<FeedRetentionPolicy>> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            FeedRetentionPolicy? policy = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                using HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}", cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while retrieving retention policy",
                            GetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while retrieving retention policy",
                            GetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            GetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to retrieve retention policy: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            GetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedRetentionPolicy? result = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(cancellationToken);
                return result;
            }, GetRetentionPolicyOperation, OperationType.Read);

            return policy == null
                ? AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(HttpStatusCode.NotFound, "No retention policy found for the specified feed.", Logger)
                : AzureDevOpsActionResult<FeedRetentionPolicy>.Success(policy, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex, Logger);
        }
    }

    /// <summary>
    /// Configures or updates the retention policy for a feed to automatically manage package lifecycle.
    /// Retention policies help control storage costs and maintain feed hygiene by automatically removing
    /// old package versions based on criteria like age, version count, or download patterns.
    /// Use with caution as this can result in automatic deletion of packages.
    /// </summary>
    /// <param name="feedId">The unique identifier of the feed for which to set the retention policy.</param>
    /// <param name="policy">The retention policy configuration including rules and criteria for package cleanup.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AzureDevOpsActionResult{T}"/> containing the updated <see cref="FeedRetentionPolicy"/> if successful,
    /// or error details if the operation fails or the policy configuration is invalid.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty or policy is null or invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify feed settings.</exception>
    public async Task<AzureDevOpsActionResult<FeedRetentionPolicy>> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            FeedRetentionPolicy updated = await ExecuteWithExceptionHandlingAsync(async () =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };
                using HttpResponseMessage response = await HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}", policy, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);

                    AzureDevOpsException exception = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => new AzureDevOpsAuthenticationException(
                            "Authentication failed while setting retention policy",
                            SetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.Forbidden => new AzureDevOpsAuthenticationException(
                            "Access denied while setting retention policy",
                            SetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.NotFound => new AzureDevOpsResourceNotFoundException(
                            $"Feed with ID '{feedId}' not found",
                            FeedResourceType,
                            feedId.ToString(),
                            SetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        HttpStatusCode.BadRequest => new AzureDevOpsApiException(
                            "Invalid retention policy configuration",
                            (int)response.StatusCode,
                            error,
                            SetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8]),
                        _ => new AzureDevOpsApiException(
                            $"Failed to set retention policy: {response.StatusCode} - {response.ReasonPhrase}",
                            (int)response.StatusCode,
                            error,
                            SetRetentionPolicyOperation,
                            Guid.NewGuid().ToString("N")[..8])
                    };

                    throw exception;
                }

                FeedRetentionPolicy? result = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(options, cancellationToken);
                return result!;
            }, SetRetentionPolicyOperation, OperationType.Update);

            return AzureDevOpsActionResult<FeedRetentionPolicy>.Success(updated, Logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex, Logger);
        }
    }
}
