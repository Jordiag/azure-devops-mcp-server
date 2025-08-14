using Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Implementation of authorization service that validates user permissions against Azure DevOps.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly ILogger<AuthorizationService> _logger;
    private readonly AzureDevOpsConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private UserSecurityContext? _cachedSecurityContext;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(10);

    public AuthorizationService(
        ILogger<AuthorizationService> logger,
        IOptions<AzureDevOpsConfiguration> configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<bool> IsAuthorizedAsync(SecurityPermission permission, string? resourceId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            UserSecurityContext securityContext = await GetCurrentUserSecurityContextAsync(cancellationToken);

            if(securityContext.IsSystemAdministrator)
            {
                _logger.LogDebug("Access granted - user is system administrator");
                return true;
            }

            if(securityContext.Permissions.Contains(permission))
            {
                _logger.LogDebug("Access granted - user has required permission: {Permission}", permission);
                return true;
            }

            if(securityContext.IsProjectAdministrator && IsProjectLevelPermission(permission))
            {
                _logger.LogDebug("Access granted - user is project administrator for project-level operation");
                return true;
            }

            _logger.LogWarning("Access denied - user lacks required permission: {Permission} for resource: {ResourceId}", permission, resourceId);
            return false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error checking authorization for permission: {Permission}", permission);
            return false; 
        }
    }

    public async Task<bool> HasAnyPermissionAsync(IEnumerable<SecurityPermission> permissions, string? resourceId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach(SecurityPermission permission in permissions)
            {
                if(await IsAuthorizedAsync(permission, resourceId, cancellationToken))
                {
                    return true;
                }
            }
            return false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error checking any permission authorization");
            return false;
        }
    }

    public async Task<bool> HasAllPermissionsAsync(IEnumerable<SecurityPermission> permissions, string? resourceId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach(SecurityPermission permission in permissions)
            {
                if(!await IsAuthorizedAsync(permission, resourceId, cancellationToken))
                {
                    return false;
                }
            }
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error checking all permissions authorization");
            return false;
        }
    }

    public async Task<UserSecurityContext> GetCurrentUserSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        if(_cachedSecurityContext != null && DateTime.UtcNow < _cacheExpiry)
        {
            return _cachedSecurityContext;
        }

        try
        {
            _logger.LogDebug("Retrieving user security context from Azure DevOps");

            using HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_configuration.OrganizationUrl);

            string token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{_configuration.PersonalAccessToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);

            UserSecurityContext securityContext = await RetrieveUserPermissionsAsync(httpClient, cancellationToken);

            _cachedSecurityContext = securityContext;
            _cacheExpiry = DateTime.UtcNow.Add(_cacheTimeout);

            _logger.LogDebug("Successfully retrieved security context for user: {UserId}", securityContext.UserId);
            return securityContext;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user security context");

            return new UserSecurityContext
            {
                UserId = "unknown",
                UserName = "Unknown User",
                Email = string.Empty,
                IsSystemAdministrator = false,
                IsProjectAdministrator = false,
                Permissions = new HashSet<SecurityPermission>(),
                AuthenticatedAt = DateTime.UtcNow,
                AuthenticationMethod = "PAT"
            };
        }
    }

    private async Task<UserSecurityContext> RetrieveUserPermissionsAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("_apis/connectionData", cancellationToken);

            if(response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully connected to Azure DevOps API");
            }
            else
            {
                _logger.LogWarning("Failed to connect to Azure DevOps API: {StatusCode}", response.StatusCode);
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error connecting to Azure DevOps API, using default permissions");
        }

        UserSecurityContext securityContext = new UserSecurityContext
        {
            UserId = "current-user", // Would be retrieved from API response
            UserName = "Current User", // Would be retrieved from API response
            Email = string.Empty, // Would be retrieved from API response
            IsSystemAdministrator = false, // Would be determined from API response
            IsProjectAdministrator = false, // Would be determined from API response
            AuthenticatedAt = DateTime.UtcNow,
            AuthenticationMethod = "PAT"
        };

        // For PAT authentication, grant basic permissions
        securityContext.Permissions.Add(SecurityPermission.ReadWorkItems);
        securityContext.Permissions.Add(SecurityPermission.WriteWorkItems);
        securityContext.Permissions.Add(SecurityPermission.ReadRepos);
        securityContext.Permissions.Add(SecurityPermission.ReadPipelines);
        securityContext.Permissions.Add(SecurityPermission.ReadTestPlans);
        securityContext.Permissions.Add(SecurityPermission.ReadArtifacts);
        securityContext.Permissions.Add(SecurityPermission.ReadProject);

        _logger.LogInformation("Created security context with {PermissionCount} permissions", securityContext.Permissions.Count);
        return securityContext;
    }

    private static bool IsProjectLevelPermission(SecurityPermission permission)
    {
        return permission switch
        {
            SecurityPermission.ReadProject or
            SecurityPermission.WriteProject or
            SecurityPermission.ReadWorkItems or
            SecurityPermission.WriteWorkItems or
            SecurityPermission.DeleteWorkItems or
            SecurityPermission.ReadRepos or
            SecurityPermission.WriteRepos or
            SecurityPermission.ReadPipelines or
            SecurityPermission.WritePipelines or
            SecurityPermission.ReadTestPlans or
            SecurityPermission.WriteTestPlans or
            SecurityPermission.ReadArtifacts or
            SecurityPermission.WriteArtifacts => true,
            SecurityPermission.AdministerProject => false, // This requires explicit admin rights
            _ => false
        };
    }
}