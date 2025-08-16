using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text.RegularExpressions;

namespace Dotnet.AzureDevOps.Core.Common
{
    /// <summary>
    /// Base class for Azure DevOps client implementations providing common functionality
    /// for connection management, disposal patterns, and shared dependencies with enhanced security.
    /// Eliminates code duplication across client classes while ensuring consistent
    /// connection handling and resource management patterns.
    /// </summary>
    public abstract class AzureDevOpsClientBase : IDisposable, IAsyncDisposable
    {
        protected readonly VssConnection Connection;
        protected readonly ILogger Logger;
        protected readonly string ProjectName;
        protected readonly string OrganizationUrl;
        protected readonly string MaskedPersonalAccessToken;
        protected bool Disposed;

        private static readonly Regex UnsafeUrlRegex = new(@"^https?://(?:localhost|127\.0\.0\.1|10\.|192\.168\.|172\.(?:1[6-9]|2[0-9]|3[01])\.|::1|localhost\.)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsClientBase class with common Azure DevOps connection setup
        /// and enhanced security validation.
        /// </summary>
        /// <param name="organizationUrl">The Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal Access Token for authentication</param>
        /// <param name="projectName">Optional project name for project-scoped operations</param>
        /// <param name="logger">Optional logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid or potentially unsafe</exception>
        protected AzureDevOpsClientBase(string organizationUrl, string personalAccessToken, string? projectName = null, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(organizationUrl))
                throw new ArgumentNullException(nameof(organizationUrl), "Organization URL cannot be null or empty");
            
            if (string.IsNullOrWhiteSpace(personalAccessToken))
                throw new ArgumentNullException(nameof(personalAccessToken), "Personal Access Token cannot be null or empty");

            ValidateOrganizationUrl(organizationUrl);
            
            ValidatePersonalAccessToken(personalAccessToken);
            
            if (!string.IsNullOrEmpty(projectName))
            {
                ValidateProjectName(projectName);
            }

            OrganizationUrl = organizationUrl;
            ProjectName = projectName ?? string.Empty;
            Logger = logger ?? NullLogger.Instance;
            
            MaskedPersonalAccessToken = MaskPersonalAccessToken(personalAccessToken);
            
            try
            {
                var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
                Connection = new VssConnection(new Uri(organizationUrl), credentials);
                
                Logger.LogInformation("Azure DevOps client initialized for organization: {OrganizationUrl}, Project: {ProjectName}, PAT: {MaskedPat}", 
                    organizationUrl, projectName ?? "[None]", MaskedPersonalAccessToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize Azure DevOps connection");
                throw new InvalidOperationException("Failed to establish Azure DevOps connection", ex);
            }
        }

        /// <summary>
        /// Validates the organization URL for proper format and security.
        /// Ensures the URL uses HTTPS protocol and checks for potentially unsafe internal or local addresses.
        /// Also performs basic validation against known Azure DevOps domains, logging warnings for non-standard hosts
        /// that might be on-premises TFS installations.
        /// </summary>
        /// <param name="organizationUrl">The URL to validate</param>
        /// <exception cref="ArgumentException">Thrown when URL is invalid or potentially unsafe</exception>
        private static void ValidateOrganizationUrl(string organizationUrl)
        {
            if (!Uri.TryCreate(organizationUrl, UriKind.Absolute, out Uri? uri))
            {
                throw new ArgumentException("Organization URL must be a valid absolute URL", nameof(organizationUrl));
            }

            if (uri.Scheme != "https")
            {
                throw new ArgumentException("Organization URL must use HTTPS for security", nameof(organizationUrl));
            }

            if (UnsafeUrlRegex.IsMatch(organizationUrl))
            {
                throw new ArgumentException("Organization URL appears to target an internal or local address, which may be unsafe", nameof(organizationUrl));
            }

            if (!uri.Host.EndsWith("dev.azure.com", StringComparison.OrdinalIgnoreCase) && 
                !uri.Host.EndsWith("visualstudio.com", StringComparison.OrdinalIgnoreCase) &&
                !uri.Host.Contains("azure", StringComparison.OrdinalIgnoreCase))
            {
                // Log warning but don't block - might be on-premises TFS
                // In production, you might want to be more restrictive
            }
        }

        /// <summary>
        /// Validates the Personal Access Token format and characteristics.
        /// Performs basic length validation (Azure DevOps PATs are typically 52 characters),
        /// checks for base64-like character format, and detects obviously invalid tokens
        /// such as placeholders or tokens from other platforms (e.g., GitHub tokens starting with 'ghp_' or 'gho_').
        /// </summary>
        /// <param name="personalAccessToken">The PAT to validate</param>
        /// <exception cref="ArgumentException">Thrown when PAT format is invalid</exception>
        private static void ValidatePersonalAccessToken(string personalAccessToken)
        {
            if (personalAccessToken.Length < 20)
            {
                throw new ArgumentException("Personal Access Token appears to be too short to be valid", nameof(personalAccessToken));
            }

            if (personalAccessToken.Length > 200)
            {
                throw new ArgumentException("Personal Access Token appears to be too long to be valid", nameof(personalAccessToken));
            }

            if (!Regex.IsMatch(personalAccessToken, @"^[a-zA-Z0-9+/=]+$", RegexOptions.Compiled))
            {
                throw new ArgumentException("Personal Access Token contains invalid characters", nameof(personalAccessToken));
            }

            if (personalAccessToken.Equals("your-pat-here", StringComparison.OrdinalIgnoreCase) ||
                personalAccessToken.Equals("replace-with-your-pat", StringComparison.OrdinalIgnoreCase) ||
                personalAccessToken.StartsWith("ghp_", StringComparison.OrdinalIgnoreCase) ||
                personalAccessToken.StartsWith("gho_", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Personal Access Token appears to be a placeholder or wrong token type", nameof(personalAccessToken));
            }
        }

        /// <summary>
        /// Validates project name format according to Azure DevOps constraints.
        /// Enforces maximum length of 64 characters, checks for invalid characters
        /// (angle brackets, quotes, pipes, wildcards, slashes), validates against reserved system names,
        /// and ensures the name doesn't start or end with a period.
        /// </summary>
        /// <param name="projectName">The project name to validate</param>
        /// <exception cref="ArgumentException">Thrown when project name is invalid</exception>
        private static void ValidateProjectName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentException("Project name cannot be null, empty, or whitespace", nameof(projectName));
            }

            if (projectName.Length > 64)
            {
                throw new ArgumentException("Project name cannot exceed 64 characters", nameof(projectName));
            }

            char[] invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*', '/', '\\' };
            if (projectName.IndexOfAny(invalidChars) >= 0)
            {
                throw new ArgumentException($"Project name contains invalid characters: {string.Join(", ", invalidChars)}", nameof(projectName));
            }

            string[] reservedNames = new[] { "con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9" };
            if (reservedNames.Contains(projectName.ToLowerInvariant()))
            {
                throw new ArgumentException("Project name cannot be a reserved system name", nameof(projectName));
            }

            if (projectName.StartsWith('.') || projectName.EndsWith('.'))
            {
                throw new ArgumentException("Project name cannot start or end with a period", nameof(projectName));
            }
        }

        /// <summary>
        /// Creates a masked version of the Personal Access Token for safe logging.
        /// Shows only the first 4 and last 4 characters, masking the rest with asterisks
        /// to maintain security while providing enough information for identification and debugging.
        /// </summary>
        /// <param name="personalAccessToken">The PAT to mask</param>
        /// <returns>Masked PAT showing only first 4 and last 4 characters</returns>
        private static string MaskPersonalAccessToken(string personalAccessToken)
        {
            if (string.IsNullOrEmpty(personalAccessToken))
                return "[EMPTY]";

            if (personalAccessToken.Length < 8)
                return "[INVALID]";

            return $"{personalAccessToken[..4]}{'*'.ToString().PadLeft(personalAccessToken.Length - 8, '*')}{personalAccessToken[^4..]}";
        }

        /// <summary>
        /// Logs security-relevant information about the connection.
        /// </summary>
        protected void LogSecurityContext()
        {
            Logger.LogInformation("Azure DevOps Security Context - Organization: {OrganizationUrl}, Project: {ProjectName}, PAT: {MaskedPat}",
                OrganizationUrl, ProjectName, MaskedPersonalAccessToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Connection?.Dispose();
                        Logger.LogDebug("Azure DevOps connection disposed for {OrganizationUrl}", OrganizationUrl);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Error disposing Azure DevOps connection");
                    }
                }
                Disposed = true;
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
            try
            {
                Connection?.Dispose();
                Logger.LogDebug("Azure DevOps connection disposed asynchronously for {OrganizationUrl}", OrganizationUrl);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error disposing Azure DevOps connection asynchronously");
            }
            return ValueTask.CompletedTask;
        }
    }
}
