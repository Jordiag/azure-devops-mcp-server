using System.Text;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public class IdentityClient : AzureDevOpsClientBase, IIdentityClient
    {
        private readonly IdentityHttpClient _identityHttpClient;

        public IdentityClient(string organizationUrl, string personalAccessToken, ILogger? logger = null)
            : base(organizationUrl, personalAccessToken, null, logger)
        {
            _identityHttpClient = Connection.GetClient<IdentityHttpClient>();
            _ = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        }

        public async Task<AzureDevOpsActionResult<(string localId, string displayName)>> GetUserLocalIdFromEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                (string localId, string displayName) tuple = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    IdentitiesCollection identities = await _identityHttpClient.ReadIdentitiesAsync(IdentitySearchFilter.General, email, cancellationToken: cancellationToken);
                    Identity? identity = identities?.FirstOrDefault(i => i.Properties != null && i.Properties.ContainsKey("Mail") && string.Equals(i.Properties["Mail"].ToString(), email, StringComparison.OrdinalIgnoreCase));
                    if(identity == null)
                        throw new InvalidOperationException($"I Couldn't find an identity from that email: {email}");
                    return (identity.Id.ToString(), identity.DisplayName ?? string.Empty);
                }, "GetUserLocalIdFromEmail", OperationType.Read);

                return AzureDevOpsActionResult<(string localId, string displayName)>.Success(tuple, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<(string localId, string displayName)>.Failure(ex, Logger);
            }
        }
    }
}