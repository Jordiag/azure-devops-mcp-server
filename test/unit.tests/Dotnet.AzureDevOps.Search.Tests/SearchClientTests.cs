using System.Net;
using System.Net.Http;
using System.Threading;
using Dotnet.AzureDevOps.Core.Search;
using Dotnet.AzureDevOps.Core.Search.Options;
using Dotnet.AzureDevOps.Core.Common;

public class SearchClientTests
{
    private static HttpClient CreateClient(HttpStatusCode statusCode, string content)
    {
        var handler = new StubHandler((request, ct) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };
            return Task.FromResult(response);
        });
        return new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
    }

    [Fact]
    public async Task SearchWikiAsync_ReturnsSuccess_OnHttp200()
    {
        HttpClient httpClient = CreateClient(HttpStatusCode.OK, "result");
        var client = new SearchClient("org", "pat", httpClient);
        var result = await client.SearchWikiAsync(new WikiSearchOptions { SearchText = "foo" });
        Assert.True(result.IsSuccess);
        Assert.Equal("result", result.Value);
    }

    [Fact]
    public async Task SearchWikiAsync_ReturnsFailure_OnHttpError()
    {
        HttpClient httpClient = CreateClient(HttpStatusCode.InternalServerError, "bad");
        var client = new SearchClient("org", "pat", httpClient);
        var result = await client.SearchWikiAsync(new WikiSearchOptions { SearchText = "foo" });
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
