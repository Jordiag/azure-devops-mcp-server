using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Feed.WebApi;
using Microsoft.VisualStudio.Services.NuGet.WebApi;
using Microsoft.VisualStudio.Services.Packaging.Shared.WebApi;


namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : IArtifactsClient
{
    private const string ApiVersion = Constants.ApiVersion;

    private readonly string _projectName;
    private readonly HttpClient _http;
    private readonly string _organizationUrl;
    private readonly FeedHttpClient _feedHttpClient;
    private readonly NuGetHttpClient _nuGetHttpClient;

    public ArtifactsClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName.TrimEnd('/');

        _organizationUrl = organizationUrl.Replace("https://dev.azure.com", "https://feeds.dev.azure.com");

        _http = new HttpClient { BaseAddress = new Uri(organizationUrl) };

        string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);

        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        var connection = new VssConnection(new Uri(organizationUrl), credentials);
        _feedHttpClient = connection.GetClient<FeedHttpClient>();
        _nuGetHttpClient = connection.GetClient<NuGetHttpClient>();
    }

    public async Task<Guid> CreateFeedAsync(FeedCreateOptions feedCreateOptions, CancellationToken cancellationToken = default)
    {
        string feedsUrl = $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds?api-version={ApiVersion}";
        var payload = new { name = feedCreateOptions.Name, description = feedCreateOptions.Description };
        HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync(
            requestUri: feedsUrl,
            value: payload,
            cancellationToken: cancellationToken);
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
            $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}?api-version={ApiVersion}")
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
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Feed>(cancellationToken);
    }

    public async Task<IReadOnlyList<Feed>> ListFeedsAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        FeedList? list = await response.Content.ReadFromJsonAsync<FeedList>(cancellationToken);
        return list?.Value?.ToArray() ?? [];
    }

    public async Task DeleteFeedAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.DeleteAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Package>> ListPackagesAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.GetAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}/packages?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        PackageList? list = await response.Content.ReadFromJsonAsync<PackageList>(cancellationToken);
        return list?.Value as IReadOnlyList<Package> ?? [];
    }

    public async Task DeletePackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _http.DeleteAsync(
            requestUri: $"{_organizationUrl}/{_projectName}/_apis/packaging/feeds/{feedId}/packages/{packageName}/versions/{version}?api-version={ApiVersion}",
            cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<FeedPermission>> GetFeedPermissionsAsync(Guid feedId, CancellationToken cancellationToken = default)
    {
        List<FeedPermission> permissions = await _feedHttpClient.GetFeedPermissionsAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            includeIds: true,
            excludeInheritedPermissions: false,
            identityDescriptor: null!,
            includeDeletedFeeds: false,
            userState: null,
            cancellationToken: cancellationToken);
        return permissions;
    }

    public async Task SetFeedPermissionsAsync(Guid feedId, IEnumerable<FeedPermission> feedPermissions, CancellationToken cancellationToken = default)
    {
        _ = await _feedHttpClient.SetFeedPermissionsAsync(
            feedPermission: feedPermissions.ToList(),
            project: _projectName,
            feedId: feedId.ToString(),
            userState: null,
            cancellationToken: cancellationToken);
    }

    public async Task<FeedView> CreateFeedViewAsync(Guid feedId, FeedView feedView, CancellationToken cancellationToken = default) =>
        await _feedHttpClient.CreateFeedViewAsync(
            view: feedView,
            project: _projectName,
            feedId: feedId.ToString(),
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<FeedView>> ListFeedViewsAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        await _feedHttpClient.GetFeedViewsAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            userState: null,
            cancellationToken: cancellationToken);

    public async Task DeleteFeedViewAsync(Guid feedId, string viewId, CancellationToken cancellationToken = default) =>
        await _feedHttpClient.DeleteFeedViewAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            viewId: viewId,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task SetUpstreamingBehaviorAsync(Guid feedId, string packageName, UpstreamingBehavior behavior, CancellationToken cancellationToken = default) =>
        await _nuGetHttpClient.SetUpstreamingBehaviorAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            packageName: packageName,
            body: behavior,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<UpstreamingBehavior> GetUpstreamingBehaviorAsync(Guid feedId, string packageName, CancellationToken cancellationToken = default) =>
        await _nuGetHttpClient.GetUpstreamingBehaviorAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            packageName: packageName,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<Package> GetPackageVersionAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default) =>
        await _nuGetHttpClient.GetPackageVersionAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            packageName: packageName,
            packageVersion: version,
            showDeleted: false,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task UpdatePackageVersionAsync(Guid feedId, string packageName, string version, PackageVersionDetails details, CancellationToken cancellationToken = default) =>
        await _nuGetHttpClient.UpdatePackageVersionAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            packageName: packageName,
            packageVersion: version,
            body: details,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<Stream> DownloadPackageAsync(Guid feedId, string packageName, string version, CancellationToken cancellationToken = default) =>
        await _nuGetHttpClient.DownloadPackageAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            packageName: packageName,
            packageVersion: version,
            sourceProtocolVersion: null,
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<FeedRetentionPolicy> GetRetentionPolicyAsync(Guid feedId, CancellationToken cancellationToken = default) =>
        await _feedHttpClient.GetFeedRetentionPoliciesAsync(
            project: _projectName,
            feedId: feedId.ToString(),
            userState: null,
            cancellationToken: cancellationToken);

    public async Task<FeedRetentionPolicy> SetRetentionPolicyAsync(Guid feedId, FeedRetentionPolicy policy, CancellationToken cancellationToken = default) =>
        await _feedHttpClient.SetFeedRetentionPoliciesAsync(
            feedRetentionPolicy: policy,
            project: _projectName,
            feedId: feedId.ToString(),
            userState: null,
            cancellationToken: cancellationToken);

}