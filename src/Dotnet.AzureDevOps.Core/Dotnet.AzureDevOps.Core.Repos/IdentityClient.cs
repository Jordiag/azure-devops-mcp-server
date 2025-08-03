using System.Text;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public class IdentityClient : IIdentitiyClient
    {
        private readonly IdentityHttpClient _identityHttpClient;

        public IdentityClient(string organizationUrl, string personalAccessToken)
        {
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _identityHttpClient = connection.GetClient<IdentityHttpClient>();

            _ = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));

        }

        public async Task<(string localId, string displayName)> GetUserLocalIdFromEmailAsync(
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
                    return (string.Empty, string.Empty);
                }

                // This gives you the actual local ID
                return (identity.Id.ToString(), identity.DisplayName ?? string.Empty);
            }
            catch
            {
                return (string.Empty, string.Empty);
            }
        }
    }
}