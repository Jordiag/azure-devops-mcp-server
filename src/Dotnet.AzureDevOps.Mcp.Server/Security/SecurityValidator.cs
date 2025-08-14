namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Attribute to require specific permissions for MCP tool methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute
{
    public SecurityPermission Permission { get; }
    public string? ResourceId { get; set; }

    public RequirePermissionAttribute(SecurityPermission permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Attribute to require any of the specified permissions for MCP tool methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireAnyPermissionAttribute : Attribute
{
    public SecurityPermission[] Permissions { get; }
    public string? ResourceId { get; set; }

    public RequireAnyPermissionAttribute(params SecurityPermission[] permissions)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }
}

/// <summary>
/// Utility class for validating permissions in MCP tools.
/// </summary>
public static class SecurityValidator
{
    /// <summary>
    /// Validates that the current user has the required permission.
    /// Throws UnauthorizedAccessException if not authorized.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve authorization service</param>
    /// <param name="permission">Required permission</param>
    /// <param name="resourceId">Optional resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks required permission</exception>
    public static async Task ValidatePermissionAsync(
        IServiceProvider serviceProvider,
        SecurityPermission permission,
        string? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        IAuthorizationService authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        ILogger<IAuthorizationService> logger = serviceProvider.GetRequiredService<ILogger<IAuthorizationService>>();

        try
        {
            bool isAuthorized = await authorizationService.IsAuthorizedAsync(permission, resourceId, cancellationToken);

            if(!isAuthorized)
            {
                logger.LogWarning("Access denied - user lacks required permission: {Permission} for resource: {ResourceId}", permission, resourceId);
                throw new UnauthorizedAccessException($"Insufficient permissions to perform this operation. Required permission: {permission}");
            }

            logger.LogDebug("Permission validation successful for {Permission}", permission);
        }
        catch(UnauthorizedAccessException)
        {
            throw; // Re-throw authorization exceptions
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error during permission validation for {Permission}", permission);
            throw new UnauthorizedAccessException("Permission validation failed", ex);
        }
    }

    /// <summary>
    /// Validates that the current user has any of the specified permissions.
    /// Throws UnauthorizedAccessException if not authorized.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve authorization service</param>
    /// <param name="permissions">Array of acceptable permissions</param>
    /// <param name="resourceId">Optional resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks any of the required permissions</exception>
    public static async Task ValidateAnyPermissionAsync(
        IServiceProvider serviceProvider,
        SecurityPermission[] permissions,
        string? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        IAuthorizationService authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        ILogger<IAuthorizationService> logger = serviceProvider.GetRequiredService<ILogger<IAuthorizationService>>();

        try
        {
            bool hasPermission = await authorizationService.HasAnyPermissionAsync(permissions, resourceId, cancellationToken);

            if(!hasPermission)
            {
                logger.LogWarning("Access denied - user lacks any required permissions: {Permissions} for resource: {ResourceId}",
                    string.Join(", ", permissions), resourceId);
                throw new UnauthorizedAccessException($"Insufficient permissions to perform this operation. Required any of: {string.Join(", ", permissions)}");
            }

            logger.LogDebug("Permission validation successful for any of {Permissions}", string.Join(", ", permissions));
        }
        catch(UnauthorizedAccessException)
        {
            throw; // Re-throw authorization exceptions
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error during permission validation for any of {Permissions}", string.Join(", ", permissions));
            throw new UnauthorizedAccessException("Permission validation failed", ex);
        }
    }

    /// <summary>
    /// Validates input parameters to ensure they are safe and properly formatted.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve sanitizer service</param>
    /// <param name="parameters">Dictionary of parameter names and values to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is invalid</exception>
    public static void ValidateInputParameters(IServiceProvider serviceProvider, Dictionary<string, object?> parameters)
    {
        IInputSanitizer inputSanitizer = serviceProvider.GetRequiredService<IInputSanitizer>();
        ILogger<IInputSanitizer> logger = serviceProvider.GetRequiredService<ILogger<IInputSanitizer>>();

        foreach(KeyValuePair<string, object?> param in parameters)
        {
            if(param.Value == null)
                continue;

            string? paramValue = param.Value.ToString();
            if(string.IsNullOrEmpty(paramValue))
                continue;

            try
            {
                // Validate the parameter value
                System.ComponentModel.DataAnnotations.ValidationResult? validationResult = inputSanitizer.ValidateInput(paramValue);
                if(validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    logger.LogWarning("Invalid parameter value for {ParameterName}: {Error}", param.Key, validationResult?.ErrorMessage);
                    throw new ArgumentException($"Invalid parameter '{param.Key}': {validationResult?.ErrorMessage}");
                }
            }
            catch(ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error validating parameter {ParameterName}", param.Key);
                throw new ArgumentException($"Parameter validation failed for '{param.Key}'", ex);
            }
        }
    }
}