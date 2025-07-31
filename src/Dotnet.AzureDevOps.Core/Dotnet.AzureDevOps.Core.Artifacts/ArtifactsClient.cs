using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;


namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : IArtifactsClient
{
    private const string ApiVersion = GlobalConstants.ApiVersion;

    private readonly string _projectName;
    private readonly HttpClient _http;
    private readonly string _organizationUrl;

    public ArtifactsClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName.TrimEnd('/');

        _organizationUrl = organizationUrl.Replace("https://dev.azure.com", "https://feeds.dev.azure.com");

        _http = new HttpClient { BaseAddress = new Uri(organizationUrl) };

        string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);

    }

    public async Task<Guid> CreateFeedAsync(FeedCreateOptions feedCreateOptions, CancellationToken cancellationToken = default)
    {
        string feedsUrl = $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}";
        var payload = new { name = feedCreateOptions.Name, description = feedCreateOptions.Description };
        HttpResponseMessage httpResponseMessage = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
            _http,
            feedsUrl,
            payload,
            cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
        Feed? feed = await httpResponseMessage.Content.ReadFromJsonAsync<Feed>(cancellationToken);
        return feed!.Id;
    }

    public async Task UpdateFeedAsync(Guid feedId, FeedUpdateOptions feedUpdateOptions, CancellationToken cancellationToken = default)
    {
        var fields = new Dictionary<string, string?>();
        if(feedUpdateOptions.Name is { Length: > 0 })
            fields["name"] = feedUpdateOptions.Name;
        if(feedUpdateOptions.Description is { Length: > 0 })
            fields["description"] = feedUpdateOptions.Description;
        if(fields.Count == 0)
            return;

        var requestMessage = new HttpRequestMessage(HttpMethod.Patch,
            $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}")
        {
            Content = JsonContent.Create(fields)
        };
        HttpResponseMessage response = await _http.SendAsync(
            request: requestMessage,
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Feed?> GetFeedAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
    }

    public async Task<IReadOnlyList<Feed>> ListFeedsAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        FeedList? list = await response.Content.ReadFromJsonAsync<FeedList>(cancellationToken);
        return list?.Value?.ToArray() ?? [];
    }

    public async Task DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.DeleteAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Package>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        PackageList? list = await response.Content.ReadFromJsonAsync<PackageList>(cancellationToken);
        return list?.Value as IReadOnlyList<Package> ?? [];
    }

    public async Task DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.DeleteAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/packages/{packageName}/versions/{version}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/permissions?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();

        FeedPermissionList? list = await response.Content.ReadFromJsonAsync<FeedPermissionList>(options, cancellationToken);
        return list?.Value?.ToArray() ?? [];
    }

    public async Task<FeedView> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
            _http,
            $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}",
            feedView,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FeedView>(options, cancellationToken))!;
    }

    public async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        FeedViewList? list = await response.Content.ReadFromJsonAsync<FeedViewList>(cancellationToken);
        return list?.Value?.ToArray() ?? [];
    }

    public async Task DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.DeleteAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/views/{viewId}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync(
            _http,
            $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}",
            behavior,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/upstreamingbehavior?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UpstreamingBehavior>(cancellationToken))!;
    }

    public async Task<Package> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Package>(cancellationToken))!;
    }

    public async Task UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}?api-version={ApiVersion}")
        {
            Content = JsonContent.Create(details)
        };
        HttpResponseMessage response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/nuget/packages/{packageName}/versions/{version}/content?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(cancellationToken))!;
    }

    public async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync(
            _http,
            $"{_organizationUrl}/{_projectName}/_apis/packaging/Feeds/{feedId}/retentionpolicies?api-version={ApiVersion}",
            policy,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FeedRetentionPolicy>(options, cancellationToken))!;
    }
}
