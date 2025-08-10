using System.Text;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public class IdentityClient : IIdentityClient, IDisposable, IAsyncDisposable
    {
        private readonly IdentityHttpClient _identityHttpClient;
        private readonly VssConnection _connection;
        private readonly ILogger? _logger;
        private bool _disposed;

        public IdentityClient(string organizationUrl, string personalAccessToken, ILogger? logger = null)
        {
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(organizationUrl), credentials);
            _logger = logger ?? NullLogger.Instance;
            _identityHttpClient = _connection.GetClient<IdentityHttpClient>();

            _ = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));

        }

        public async Task<AzureDevOpsActionResult<(string localId, string displayName)>> GetUserLocalIdFromEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Search for identity by email
                IdentitiesCollection identities = await _identityHttpClient.ReadIdentitiesAsync(
                    IdentitySearchFilter.General,
                    email,
                    cancellationToken: cancellationToken);

                Identity? identity = identities?.FirstOrDefault(i =>
                    i.Properties != null &&
                    i.Properties.ContainsKey("Mail") &&
                    string.Equals(i.Properties["Mail"].ToString(), email, StringComparison.OrdinalIgnoreCase));

                if(identity == null)
                {
                    return AzureDevOpsActionResult<(string localId, string displayName)>.Failure($"I Couldn't find an identity from that email: {email}");
                }

                // This gives you the actual local ID
                return AzureDevOpsActionResult<(string localId, string displayName)>.Success((identity.Id.ToString(), identity.DisplayName ?? string.Empty), _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<(string localId, string displayName)>.Failure(ex, _logger);
            }
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