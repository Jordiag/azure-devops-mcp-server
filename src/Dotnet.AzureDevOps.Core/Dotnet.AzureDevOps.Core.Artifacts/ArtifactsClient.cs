using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : IArtifactsClient
{
    private const string ApiVersion = GlobalConstants.ApiVersion;

    private readonly string _projectName;
    private readonly HttpClient _httpClient;
    private readonly string _organizationUrl;
    private readonly ILogger _logger;

    public ArtifactsClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        _projectName = projectName.TrimEnd('/');
        _logger = logger ?? NullLogger.Instance;
        _organizationUrl = organizationUrl.Replace("https://dev.azure.com", "https://feeds.dev.azure.com");
        _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };
        string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);
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
            string feedsUrl = $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}";
            object payload = new { name = feedCreateOptions.Name, description = feedCreateOptions.Description };
            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, feedsUrl, payload, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<Guid>.Failure(response.StatusCode, error, _logger);
            }
            Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
            return AzureDevOpsActionResult<Guid>.Success(feed!.Id, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
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
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}")
            {
                Content = JsonContent.Create(fields)
            };
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<Feed>.Failure(response.StatusCode, error, _logger);
            }
            Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
            return AzureDevOpsActionResult<Feed>.Success(feed!, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Feed>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Failure(response.StatusCode, error, _logger);
            }
            FeedList? list = await response.Content.ReadFromJsonAsync<FeedList>(cancellationToken);
            IReadOnlyList<Feed> feeds = list?.Value?.ToArray() ?? Array.Empty<Feed>();
            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Success(feeds, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<Package>>.Failure(response.StatusCode, error, _logger);
            }
            PackageList? list = await response.Content.ReadFromJsonAsync<PackageList>(cancellationToken);
            IReadOnlyList<Package> packages = list?.Value?.ToArray() ?? Array.Empty<Package>();
            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Success(packages, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages/{packageName}/versions/{version}?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/permissions?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Failure(response.StatusCode, error, _logger);
            }
            FeedPermissionList? list = await response.Content.ReadFromJsonAsync<FeedPermissionList>(options, cancellationToken);
            IReadOnlyList<FeedPermission> permissions = list?.Value?.ToArray() ?? Array.Empty<FeedPermission>();
            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Success(permissions, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Failure(ex, _logger);
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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}", feedView, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<FeedView>.Failure(response.StatusCode, error, _logger);
            }
            FeedView? created = await response.Content.ReadFromJsonAsync<FeedView>(options, cancellationToken);
            return AzureDevOpsActionResult<FeedView>.Success(created!, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedView>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Failure(response.StatusCode, error, _logger);
            }
            FeedViewList? list = await response.Content.ReadFromJsonAsync<FeedViewList>(cancellationToken);
            IReadOnlyList<FeedView> views = list?.Value?.ToArray() ?? Array.Empty<FeedView>();
            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Success(views, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views/{viewId}?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
            HttpResponseMessage response = await HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}", behavior, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<UpstreamingBehavior>.Failure(response.StatusCode, error, _logger);
            }
            UpstreamingBehavior behavior = await response.Content.ReadFromJsonAsync<UpstreamingBehavior>(cancellationToken);

            return AzureDevOpsActionResult<UpstreamingBehavior>.Success(behavior, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<UpstreamingBehavior>.Failure(ex, _logger);
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
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName is null/empty, or version is null/empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the feed or package.</exception>
    public async Task<AzureDevOpsActionResult<Package>> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<Package>.Failure(response.StatusCode, error, _logger);
            }
            Package? package = await response.Content.ReadFromJsonAsync<Package>(cancellationToken);
            return AzureDevOpsActionResult<Package>.Success(package!, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Package>.Failure(ex, _logger);
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
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}")
            {
                Content = JsonContent.Create(details)
            };
            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
            }
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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
    /// The caller is responsible for disposing of the returned stream.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when feedId is empty, packageName is null/empty, or version is null/empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to download from the feed.</exception>
    public async Task<AzureDevOpsActionResult<Stream>> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}/content?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<Stream>.Failure(response.StatusCode, error, _logger);
            }
            Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return AzureDevOpsActionResult<Stream>.Success(contentStream, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Stream>.Failure(ex, _logger);
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
            HttpResponseMessage response = await _httpClient.GetAsync($"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}", cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(response.StatusCode, error, _logger);
            }
            FeedRetentionPolicy? policy = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(cancellationToken);
            return policy == null
                ? AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(HttpStatusCode.NotFound, "No retention policy found for the specified feed.", _logger)
                : AzureDevOpsActionResult<FeedRetentionPolicy>.Success(policy, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex, _logger);
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
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            HttpResponseMessage response = await HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}", policy, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(response.StatusCode, error, _logger);
            }
            FeedRetentionPolicy? updated = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(options, cancellationToken);
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Success(updated!, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex, _logger);
        }
    }
}