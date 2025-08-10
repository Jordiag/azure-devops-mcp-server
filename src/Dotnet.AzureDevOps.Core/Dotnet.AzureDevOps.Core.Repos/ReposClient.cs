using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient : IReposClient, IDisposable, IAsyncDisposable
    {
        private readonly string _projectName;
        private readonly GitHttpClient _gitHttpClient;
        private readonly string _organizationUrl;
        private readonly HttpClient _httpClient;
        private readonly VssConnection _connection;
        private readonly ILogger? _logger;
        private bool _disposed;

        public ReposClient(HttpClient httpClient, string organizationUrl, string projectName, string personalAccessToken, ILogger<ReposClient>? logger = null)
        {
            _projectName = projectName;
            _organizationUrl = organizationUrl;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(organizationUrl), credentials);
            _gitHttpClient = _connection.GetClient<GitHttpClient>();
            _httpClient = httpClient;
            _logger = (ILogger?)logger ?? NullLogger.Instance;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            _connection?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}