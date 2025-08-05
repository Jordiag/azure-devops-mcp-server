using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;

namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : IArtifactsClient
{
    private const string ApiVersion = GlobalConstants.ApiVersion;

    private readonly string _projectName;
    private readonly HttpClient _httpClient;
    private readonly string _organizationUrl;

    public ArtifactsClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName.TrimEnd('/');
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
                return AzureDevOpsActionResult<Guid>.Failure(response.StatusCode, error);
            }
            Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
            return AzureDevOpsActionResult<Guid>.Success(feed!.Id);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Guid>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> UpdateFeedAsync(Guid feedId, FeedUpdateOptions feedUpdateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<string, string?> fields = new Dictionary<string, string?>();
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
                return AzureDevOpsActionResult<bool>.Success(true);
            }

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}")
            {
                Content = JsonContent.Create(fields)
            };
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<Feed>.Failure(response.StatusCode, error);
            }
            Feed? feed = await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
            return AzureDevOpsActionResult<Feed>.Success(feed!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Feed>.Failure(ex);
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
                return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Failure(response.StatusCode, error);
            }
            FeedList? list = await response.Content.ReadFromJsonAsync<FeedList>(cancellationToken);
            IReadOnlyList<Feed> feeds = list?.Value?.ToArray() ?? Array.Empty<Feed>();
            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Success(feeds);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Feed>>.Failure(ex);
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
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<IReadOnlyList<Package>>.Failure(response.StatusCode, error);
            }
            PackageList? list = await response.Content.ReadFromJsonAsync<PackageList>(cancellationToken);
            IReadOnlyList<Package> packages = list?.Value?.ToArray() ?? Array.Empty<Package>();
            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Success(packages);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<Package>>.Failure(ex);
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
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Failure(response.StatusCode, error);
            }
            FeedPermissionList? list = await response.Content.ReadFromJsonAsync<FeedPermissionList>(options, cancellationToken);
            IReadOnlyList<FeedPermission> permissions = list?.Value?.ToArray() ?? Array.Empty<FeedPermission>();
            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Success(permissions);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedPermission>>.Failure(ex);
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
                return AzureDevOpsActionResult<FeedView>.Failure(response.StatusCode, error);
            }
            FeedView? created = await response.Content.ReadFromJsonAsync<FeedView>(options, cancellationToken);
            return AzureDevOpsActionResult<FeedView>.Success(created!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedView>.Failure(ex);
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
                return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Failure(response.StatusCode, error);
            }
            FeedViewList? list = await response.Content.ReadFromJsonAsync<FeedViewList>(cancellationToken);
            IReadOnlyList<FeedView> views = list?.Value?.ToArray() ?? Array.Empty<FeedView>();
            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Success(views);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<FeedView>>.Failure(ex);
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
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<UpstreamingBehavior>.Failure(response.StatusCode, error);
            }
            UpstreamingBehavior? behavior = await response.Content.ReadFromJsonAsync<UpstreamingBehavior>(cancellationToken);
            return AzureDevOpsActionResult<UpstreamingBehavior>.Success(behavior!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<UpstreamingBehavior>.Failure(ex);
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
                return AzureDevOpsActionResult<Package>.Failure(response.StatusCode, error);
            }
            Package? package = await response.Content.ReadFromJsonAsync<Package>(cancellationToken);
            return AzureDevOpsActionResult<Package>.Success(package!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Package>.Failure(ex);
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
                return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
            }
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
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
                return AzureDevOpsActionResult<Stream>.Failure(response.StatusCode, error);
            }
            Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return AzureDevOpsActionResult<Stream>.Success(contentStream);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Stream>.Failure(ex);
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
                return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(response.StatusCode, error);
            }
            FeedRetentionPolicy? policy = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(cancellationToken);
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Success(policy!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<FeedRetentionPolicy>> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            HttpResponseMessage response = await HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}", policy, cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(response.StatusCode, error);
            }
            FeedRetentionPolicy? updated = await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(options, cancellationToken);
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Success(updated!);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<FeedRetentionPolicy>.Failure(ex);
        }
    }
}
