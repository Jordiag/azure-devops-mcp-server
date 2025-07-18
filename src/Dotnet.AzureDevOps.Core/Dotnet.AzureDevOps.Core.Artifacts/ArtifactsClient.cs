﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Dotnet.AzureDevOps.Core.Artifacts.Models;
using Dotnet.AzureDevOps.Core.Artifacts.Options;
using Dotnet.AzureDevOps.Core.Common;


namespace Dotnet.AzureDevOps.Core.Artifacts;

public class ArtifactsClient : IArtifactsClient
{
    private const string ApiVersion = Constants.ApiVersion;

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
}