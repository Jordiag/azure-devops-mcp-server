using Microsoft.Extensions.Options;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Configuration options for security headers middleware.
/// </summary>
public class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";

    /// <summary>
    /// Whether to add security headers (default: true)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Content Security Policy header value
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";

    /// <summary>
    /// X-Frame-Options header value
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// X-Content-Type-Options header value
    /// </summary>
    public string ContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// Referrer-Policy header value
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Permissions-Policy header value
    /// </summary>
    public string PermissionsPolicy { get; set; } = "camera=(), microphone=(), geolocation=()";

    /// <summary>
    /// Whether to add HSTS header
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// HSTS max-age in seconds
    /// </summary>
    public int HstsMaxAge { get; set; } = 31536000; // 1 year
}

/// <summary>
/// Middleware that adds security headers to all responses.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger,
        IOptions<SecurityHeadersOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if(_options.Enabled)
        {
            AddSecurityHeaders(context);
        }

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        try
        {
            HttpResponse response = context.Response;

            if(!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
            {
                response.Headers.TryAdd("Content-Security-Policy", _options.ContentSecurityPolicy);
            }

            if(!string.IsNullOrEmpty(_options.FrameOptions))
            {
                response.Headers.TryAdd("X-Frame-Options", _options.FrameOptions);
            }

            if(!string.IsNullOrEmpty(_options.ContentTypeOptions))
            {
                response.Headers.TryAdd("X-Content-Type-Options", _options.ContentTypeOptions);
            }

            if(!string.IsNullOrEmpty(_options.ReferrerPolicy))
            {
                response.Headers.TryAdd("Referrer-Policy", _options.ReferrerPolicy);
            }

            if(!string.IsNullOrEmpty(_options.PermissionsPolicy))
            {
                response.Headers.TryAdd("Permissions-Policy", _options.PermissionsPolicy);
            }

            if(_options.EnableHsts && context.Request.IsHttps)
            {
                response.Headers.TryAdd("Strict-Transport-Security", $"max-age={_options.HstsMaxAge}; includeSubDomains");
            }

            response.Headers.Remove("Server");
            response.Headers.Remove("X-Powered-By");
            response.Headers.Remove("X-AspNet-Version");

            response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
            response.Headers.TryAdd("Cache-Control", "no-cache, no-store, must-revalidate");
            response.Headers.TryAdd("Pragma", "no-cache");
            response.Headers.TryAdd("Expires", "0");

            _logger.LogDebug("Security headers added to response");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding security headers");
            // Don't fail the request if security headers can't be added
        }
    }
}