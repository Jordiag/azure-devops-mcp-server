using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Overview.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public class WikiClient : IWikiClient, IDisposable, IAsyncDisposable
    {
        private readonly string _projectName;
        private readonly WikiHttpClient _wikiHttpClient;
        private readonly VssConnection _connection;
        private readonly ILogger? _logger;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the WikiClient with authenticated Azure DevOps connection for comprehensive wiki and content management operations.
        /// Establishes secure connection to Azure DevOps Wiki HTTP client enabling complete wiki lifecycle management, page operations, and content collaboration.
        /// Provides dedicated access to wiki creation, content management, page operations, and collaborative documentation workflows within Azure DevOps projects.
        /// Essential for building comprehensive knowledge management solutions, documentation systems, and collaborative content creation platforms.
        /// </summary>
        /// <param name="organizationUrl">Complete URL of the Azure DevOps organization including protocol and domain for secure service connection and wiki API access.</param>
        /// <param name="projectName">Name of the specific Azure DevOps project for scoped wiki operations and project-specific content management.</param>
        /// <param name="personalAccessToken">Personal Access Token with wiki management permissions for authenticated access to wiki creation, content operations, and page management services.</param>
        /// <param name="logger">Optional logger instance for comprehensive operation tracking, error reporting, and diagnostic information during wiki operations. Uses null if not provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when organizationUrl, projectName, or personalAccessToken are null or empty</exception>
        /// <exception cref="ArgumentException">Thrown when organizationUrl is malformed, projectName contains invalid characters, or personalAccessToken format is incorrect</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the personal access token lacks required permissions for wiki operations or project access</exception>
        /// <exception cref="VssServiceException">Thrown when connection to Azure DevOps wiki services fails or organization validation encounters issues</exception>
        public WikiClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _logger = logger;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(organizationUrl), credentials);
            _wikiHttpClient = _connection.GetClient<WikiHttpClient>();
        }

        /// <summary>
        /// Creates a new wiki in Azure DevOps with comprehensive configuration including name, repository mapping, and version control integration.
        /// Establishes structured knowledge base with configurable repository backing, path mapping, and version control for collaborative documentation.
        /// Supports both project-level wikis and code-backed wikis with Git repository integration for comprehensive content management and version tracking.
        /// Essential for establishing documentation systems, knowledge sharing platforms, and collaborative content creation within Azure DevOps projects.
        /// </summary>
        /// <param name="wikiCreateOptions">Wiki configuration including name, project ID, repository ID, type, mapped path, and version specifications for comprehensive wiki setup.</param>
        /// <param name="cancellationToken">Optional token to cancel the wiki creation operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: GUID identifier of the newly created wiki for future content management and reference operations
        /// - Failure: Error details if wiki creation fails due to invalid parameters, permissions, or repository configuration issues
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when required wiki parameters like name, project ID, or repository configuration are missing or invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to create wikis in the specified project or access the backing repository</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps wiki service encounters issues during wiki creation or repository validation</exception>
        public async Task<AzureDevOpsActionResult<Guid>> CreateWikiAsync(WikiCreateOptions wikiCreateOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var wikiCreateParameters = new WikiCreateParametersV2
                {
                    Name = wikiCreateOptions.Name,
                    ProjectId = wikiCreateOptions.ProjectId,
                    RepositoryId = wikiCreateOptions.RepositoryId,
                    Type = wikiCreateOptions.Type,
                    MappedPath = wikiCreateOptions.MappedPath,
                    Version = wikiCreateOptions.Version,
                };

                WikiV2 wiki = await _wikiHttpClient.CreateWikiAsync(
                    wikiCreateParams: wikiCreateParameters,
                    project: _projectName,
                    cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Guid>.Success(wiki.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves a specific wiki from Azure DevOps by its unique identifier, providing comprehensive wiki metadata and configuration details.
        /// Returns complete WikiV2 object with wiki properties, repository information, mapping configuration, and administrative settings for wiki management.
        /// Essential for wiki inspection, configuration validation, and programmatic access to wiki details within Azure DevOps documentation systems.
        /// Enables wiki analysis, content management preparation, and comprehensive wiki information gathering for reporting and automation workflows.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki to retrieve from Azure DevOps wiki management system.</param>
        /// <param name="cancellationToken">Optional token to cancel the wiki retrieval operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: WikiV2 object with complete wiki metadata, repository configuration, mapping details, and administrative information
        /// - Failure: Error details if wiki cannot be found, access is denied, or service issues occur during wiki retrieval
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiki ID is invalid, malformed, or references a non-existent wiki</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access the specified wiki or project</exception>
        /// <exception cref="VssServiceException">Thrown when the wiki does not exist or Azure DevOps service encounters issues during wiki data retrieval</exception>
        public async Task<AzureDevOpsActionResult<WikiV2>> GetWikiAsync(Guid wikiId, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiV2 wiki = await _wikiHttpClient.GetWikiAsync(project: _projectName, wikiIdentifier: wikiId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiV2>.Success(wiki, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiV2>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves all wikis available within the Azure DevOps project, providing comprehensive overview of documentation and knowledge management resources.
        /// Returns collection of WikiV2 objects with complete wiki metadata, configurations, and repository associations for project-wide wiki inventory.
        /// Essential for wiki discovery, documentation catalog management, and comprehensive knowledge base overview within Azure DevOps projects.
        /// Enables programmatic access to all project wikis for reporting, automation, wiki migration, and documentation management workflows.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the wiki listing operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: Read-only list of WikiV2 objects with complete metadata, repository configurations, and administrative details for all project wikis
        /// - Failure: Error details if wikis cannot be retrieved due to permissions, service issues, or project access problems
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to list wikis in the specified project</exception>
        /// <exception cref="VssServiceException">Thrown when Azure DevOps wiki service encounters issues during wiki enumeration or project validation</exception>
        /// <exception cref="TimeoutException">Thrown when the operation exceeds allowed time limits due to large wiki datasets or service performance issues</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WikiV2>>> ListWikisAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<WikiV2> wikis = await _wikiHttpClient.GetAllWikisAsync(project: _projectName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<WikiV2>>.Success(wikis, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WikiV2>>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Permanently deletes a wiki from Azure DevOps, removing all associated pages, content, and configuration while preserving audit trail.
        /// Performs irreversible removal of the entire wiki structure including all pages, attachments, and version history if applicable.
        /// Critical operation for wiki lifecycle management and cleanup of obsolete or incorrect documentation systems within Azure DevOps projects.
        /// Should be used with caution as deletion cannot be undone and will impact any dependent documentation workflows or content references.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki to permanently delete from Azure DevOps wiki management system.</param>
        /// <param name="cancellationToken">Optional token to cancel the wiki deletion operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: WikiV2 object representing the deleted wiki with final state information and audit details
        /// - Failure: Error details if deletion fails due to permissions, wiki dependencies, or service issues during removal
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to delete the wiki or modify the project's documentation structure</exception>
        /// <exception cref="VssServiceException">Thrown when the wiki cannot be found, deletion is blocked by system constraints, or service issues occur</exception>
        /// <exception cref="InvalidOperationException">Thrown when the wiki is in use by active processes or has dependencies that prevent deletion</exception>
        public async Task<AzureDevOpsActionResult<WikiV2>> DeleteWikiAsync(Guid wikiId, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiV2 wiki = await _wikiHttpClient.DeleteWikiAsync(wikiIdentifier: wikiId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiV2>.Success(wiki, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiV2>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Creates a new wiki page or updates an existing page within the specified wiki, supporting comprehensive content management and version control.
        /// Handles both page creation and content updates with version tracking, path-based organization, and Git integration for collaborative editing.
        /// Supports rich markdown content, hierarchical page organization, and version-controlled content management for comprehensive documentation workflows.
        /// Essential for building and maintaining structured documentation, knowledge bases, and collaborative content creation within Azure DevOps wikis.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki where the page will be created or updated for content management operations.</param>
        /// <param name="wikiPageUpdateOptions">Page configuration including content, path, version, and update parameters for comprehensive page management and version control.</param>
        /// <param name="gitVersionDescriptor">Git version descriptor specifying branch, commit, or tag information for version-controlled content management and tracking.</param>
        /// <param name="cancellationToken">Optional token to cancel the page creation or update operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: Integer page ID of the created or updated wiki page for future content management and reference operations
        /// - Failure: Error details if page operation fails due to invalid parameters, version conflicts, permissions, or content validation issues
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when required page parameters like content, path, or version information are missing or invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to create or update pages in the specified wiki</exception>
        /// <exception cref="VssServiceException">Thrown when wiki does not exist, version conflicts occur, or Azure DevOps service encounters page management issues</exception>
        public async Task<AzureDevOpsActionResult<int>> CreateOrUpdatePageAsync(Guid wikiId, WikiPageUpdateOptions wikiPageUpdateOptions, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default)
        {
            try
            {
                var pageParameters = new WikiPageCreateOrUpdateParameters
                {
                    Content = wikiPageUpdateOptions.Content
                };

                WikiPageResponse response = await _wikiHttpClient.CreateOrUpdatePageAsync(
                    parameters: pageParameters,
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: wikiPageUpdateOptions.Path,
                    Version: wikiPageUpdateOptions.Version,
                    versionDescriptor: gitVersionDescriptor,
                    cancellationToken: cancellationToken);

                int? pageId = response.Page?.Id;
                return pageId.HasValue
                    ? AzureDevOpsActionResult<int>.Success(pageId.Value, _logger)
                    : AzureDevOpsActionResult<int>.Failure("Wiki page id is null.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves a specific wiki page by its path within the designated wiki, providing complete page content and metadata for content access.
        /// Returns comprehensive WikiPageResponse with rendered content, metadata, version information, and page properties for content consumption and analysis.
        /// Essential for content retrieval, page inspection, and programmatic access to wiki documentation within Azure DevOps knowledge management systems.
        /// Enables content analysis, documentation processing, and comprehensive page information gathering for reporting and content management workflows.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki containing the target page for content retrieval and analysis operations.</param>
        /// <param name="path">Hierarchical path of the wiki page within the wiki structure for precise page identification and content access.</param>
        /// <param name="cancellationToken">Optional token to cancel the page retrieval operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: WikiPageResponse object with complete page content, metadata, version information, and rendering details
        /// - Failure: Error details if page cannot be found, access is denied, path is invalid, or service issues occur during page retrieval
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiki ID is invalid or page path is malformed, empty, or references a non-existent page</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access the specified wiki page or wiki content</exception>
        /// <exception cref="VssServiceException">Thrown when the wiki or page does not exist or Azure DevOps service encounters issues during content retrieval</exception>
        public async Task<AzureDevOpsActionResult<WikiPageResponse>> GetPageAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPageResponse response = await _wikiHttpClient.GetPageAsync(
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: path,
                    includeContent: true,
                    cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiPageResponse>.Success(response, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiPageResponse>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves a paginated collection of wiki pages within the specified wiki, providing comprehensive page inventory with metadata and analytics.
        /// Returns detailed page information including titles, paths, view statistics, modification dates, and hierarchical organization for comprehensive wiki analysis.
        /// Supports advanced filtering, pagination, and analytics integration with configurable page view tracking for usage insights and content management.
        /// Essential for wiki content discovery, usage analysis, and comprehensive page catalog management within Azure DevOps documentation systems.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki from which to retrieve the paginated collection of pages and their detailed information.</param>
        /// <param name="pagesOptions">Pagination and filtering configuration including top count, continuation token, and page view analytics parameters for result management.</param>
        /// <param name="versionDescriptor">Optional Git version descriptor specifying branch, commit, or tag for version-specific page enumeration and content analysis.</param>
        /// <param name="cancellationToken">Optional token to cancel the page listing operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: Read-only list of WikiPageDetail objects with complete page metadata, analytics, hierarchy information, and organizational structure
        /// - Failure: Error details if pages cannot be retrieved due to permissions, invalid wiki ID, version issues, or service accessibility problems
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiki ID is invalid, pagination options are malformed, or version descriptor is incorrect</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to list pages within the specified wiki or access page metadata</exception>
        /// <exception cref="VssServiceException">Thrown when the wiki does not exist, version is invalid, or Azure DevOps service encounters issues during page enumeration</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>> ListPagesAsync(Guid wikiId, WikiPagesBatchOptions pagesOptions, GitVersionDescriptor? versionDescriptor = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new WikiPagesBatchRequest
                {
                    Top = pagesOptions.Top,
                    ContinuationToken = pagesOptions.ContinuationToken,
                    PageViewsForDays = pagesOptions.PageViewsForDays
                };

                PagedList<WikiPageDetail> pages = await _wikiHttpClient.GetPagesBatchAsync(
                    pagesBatchRequest: request,
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    versionDescriptor: versionDescriptor,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>.Success(pages, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WikiPageDetail>>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves the raw text content of a specific wiki page as a string, providing direct access to markdown source for content processing.
        /// Returns unprocessed page content enabling content analysis, text processing, migration, and programmatic content manipulation workflows.
        /// Essential for content extraction, search indexing, migration operations, and raw content analysis within Azure DevOps wiki management systems.
        /// Enables advanced content processing, backup operations, and comprehensive text-based content management for documentation automation workflows.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki containing the target page for raw content extraction and text processing operations.</param>
        /// <param name="path">Hierarchical path of the wiki page within the wiki structure for precise page identification and content access.</param>
        /// <param name="cancellationToken">Optional token to cancel the page text retrieval operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: String containing the raw text content of the wiki page for content processing and analysis operations
        /// - Failure: Error details if page text cannot be retrieved due to permissions, invalid path, content access issues, or service problems
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiki ID is invalid or page path is malformed, empty, or references a non-existent page</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to access the raw content of the specified wiki page</exception>
        /// <exception cref="VssServiceException">Thrown when the wiki or page does not exist or Azure DevOps service encounters issues during content stream retrieval</exception>
        public async Task<AzureDevOpsActionResult<string>> GetPageTextAsync(Guid wikiId, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                Stream stream = await _wikiHttpClient.GetPageTextAsync(
                    project: _projectName,
                    wikiIdentifier: wikiId,
                    path: path,
                    recursionLevel: VersionControlRecursionType.None,
                    versionDescriptor: null,
                    includeContent: true,
                    cancellationToken: cancellationToken);

                using StreamReader reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync(cancellationToken);
                return AzureDevOpsActionResult<string>.Success(content, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Permanently deletes a specific wiki page from the designated wiki, removing content and references while maintaining audit trail.
        /// Performs irreversible removal of the page including all content, attachments, and version history with proper version control integration.
        /// Critical operation for wiki content lifecycle management and cleanup of obsolete or incorrect documentation within Azure DevOps wikis.
        /// Should be used with caution as page deletion cannot be undone and will impact any dependent content references or navigation structures.
        /// </summary>
        /// <param name="wikiId">Unique GUID identifier of the wiki containing the target page for deletion and content removal operations.</param>
        /// <param name="path">Hierarchical path of the wiki page within the wiki structure for precise page identification and deletion targeting.</param>
        /// <param name="gitVersionDescriptor">Git version descriptor specifying branch, commit, or tag information for version-controlled deletion and audit trail management.</param>
        /// <param name="cancellationToken">Optional token to cancel the page deletion operation if needed before completion.</param>
        /// <returns>
        /// Task resolving to AzureDevOpsActionResult containing:
        /// - Success: WikiPageResponse object representing the deleted page with final state information and audit details
        /// - Failure: Error details if deletion fails due to permissions, version conflicts, page dependencies, or service issues during removal
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when wiki ID is invalid, page path is malformed, or version descriptor is incorrect</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions exist to delete pages from the specified wiki or modify wiki content</exception>
        /// <exception cref="VssServiceException">Thrown when wiki or page does not exist, version conflicts occur, or Azure DevOps service encounters deletion issues</exception>
        public async Task<AzureDevOpsActionResult<WikiPageResponse>> DeletePageAsync(Guid wikiId, string path, GitVersionDescriptor gitVersionDescriptor, CancellationToken cancellationToken = default)
        {
            try
            {
                WikiPageResponse response = await _wikiHttpClient.DeletePageAsync(project: _projectName, wikiIdentifier: wikiId, path: path, versionDescriptor: gitVersionDescriptor, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<WikiPageResponse>.Success(response, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<WikiPageResponse>.Failure(ex, _logger);
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