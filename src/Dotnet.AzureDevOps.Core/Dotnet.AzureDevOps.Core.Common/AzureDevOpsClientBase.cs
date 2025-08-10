using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Common
{
    /// <summary>
    /// Base class for Azure DevOps client implementations providing common functionality
    /// for connection management, disposal patterns, and shared dependencies.
    /// Eliminates code duplication across client classes while ensuring consistent
    /// connection handling and resource management patterns.
    /// </summary>
    public abstract class AzureDevOpsClientBase : IDisposable, IAsyncDisposable
    {
        protected readonly VssConnection Connection;
        protected readonly ILogger Logger;
        protected readonly string ProjectName;
        protected readonly string OrganizationUrl;
        protected bool Disposed;

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsClientBase class with common Azure DevOps connection setup.
        /// </summary>
        /// <param name="organizationUrl">The Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal Access Token for authentication</param>
        /// <param name="projectName">Optional project name for project-scoped operations</param>
        /// <param name="logger">Optional logger instance</param>
        protected AzureDevOpsClientBase(string organizationUrl, string personalAccessToken, string? projectName = null, ILogger? logger = null)
        {
            OrganizationUrl = organizationUrl;
            ProjectName = projectName ?? string.Empty;
            Logger = logger ?? NullLogger.Instance;
            
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            Connection = new VssConnection(new Uri(organizationUrl), credentials);
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
                    Connection?.Dispose();
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
            Connection?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
