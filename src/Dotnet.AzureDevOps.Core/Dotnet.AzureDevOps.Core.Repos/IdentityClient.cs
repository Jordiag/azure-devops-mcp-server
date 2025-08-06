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
    public class IdentityClient : IIdentityClient
    {
        private readonly IdentityHttpClient _identityHttpClient;
        private readonly ILogger? _logger;

        public IdentityClient(string organizationUrl, string personalAccessToken, ILogger? logger = null)
        {
            VssBasicCredential credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            VssConnection connection = new VssConnection(new Uri(organizationUrl), credentials);
            _logger = logger ?? NullLogger.Instance;
            _identityHttpClient = connection.GetClient<IdentityHttpClient>();

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
    }
}