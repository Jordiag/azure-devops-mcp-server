namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Defines available security permissions for Azure DevOps operations.
/// </summary>
public enum SecurityPermission
{
    ReadWorkItems,
    WriteWorkItems,
    DeleteWorkItems,
    ReadRepos,
    WriteRepos,
    ReadPipelines,
    WritePipelines,
    ReadTestPlans,
    WriteTestPlans,
    ReadArtifacts,
    WriteArtifacts,
    ReadProject,
    WriteProject,
    AdministerProject
}

/// <summary>
/// Provides authorization services to validate user permissions for specific operations.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Validates if the current user has the required permission for an operation.
    /// </summary>
    /// <param name="permission">The required permission</param>
    /// <param name="resourceId">Optional resource identifier for resource-specific authorization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authorized, false otherwise</returns>
    Task<bool> IsAuthorizedAsync(SecurityPermission permission, string? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the current user has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">Collection of acceptable permissions</param>
    /// <param name="resourceId">Optional resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has at least one of the permissions</returns>
    Task<bool> HasAnyPermissionAsync(IEnumerable<SecurityPermission> permissions, string? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the current user has all of the specified permissions.
    /// </summary>
    /// <param name="permissions">Collection of required permissions</param>
    /// <param name="resourceId">Optional resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has all permissions</returns>
    Task<bool> HasAllPermissionsAsync(IEnumerable<SecurityPermission> permissions, string? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user's security context and permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User security context</returns>
    Task<UserSecurityContext> GetCurrentUserSecurityContextAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the security context for a user including their permissions and identity.
/// </summary>
public class UserSecurityContext
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsSystemAdministrator { get; set; }
    public bool IsProjectAdministrator { get; set; }
    public HashSet<SecurityPermission> Permissions { get; set; } = new();
    public Dictionary<string, object> AdditionalClaims { get; set; } = new();
    public DateTime AuthenticatedAt { get; set; }
    public string AuthenticationMethod { get; set; } = string.Empty;
}