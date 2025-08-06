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

    public async Task<AzureDevOpsActionResult<RetentionPolicyResult>> SetRetentionPolicyAsync(
        Guid feedId,
        int daysToKeep,
        string[] packageTypes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RetentionPolicyResult? retentionPolicyResult = null;
            string retentionUrl = $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}/retentionpolicies?api-version=7.1-preview.1";

            var payload = new
            {
                retentionPolicy = new
                {
                    daysToKeep = daysToKeep,
                    deleteUnreferenced = true,
                    applyToAllVersions = true,
                    packageTypes = packageTypes,
                    filters = Array.Empty<object>()
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(
                _httpClient, retentionUrl, payload, cancellationToken);

            if(response.IsSuccessStatusCode)
            {
                retentionPolicyResult = await response.Content.ReadFromJsonAsync<RetentionPolicyResult>(cancellationToken);
                return retentionPolicyResult == null
                    ? AzureDevOpsActionResult<RetentionPolicyResult>.Failure("Retention policy deserialization gave a null value on SetRetentionPolicy.", _logger)
                    : AzureDevOpsActionResult<RetentionPolicyResult>.Success(retentionPolicyResult, _logger);
            }
            else
            {
                return AzureDevOpsActionResult<RetentionPolicyResult>.Failure(response.StatusCode, "Retention policy query failed on SetRetentionPolicy.", _logger);
            }
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<RetentionPolicyResult>.Failure(ex, _logger);
        }
    }


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

    public async Task<AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
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

    public async Task<AzureDevOpsActionResult<FeedView>> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default)
    {
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
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

    public async Task<AzureDevOpsActionResult<bool>> UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}")
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