using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Implementation of input sanitization and validation services to prevent injection attacks
/// and ensure data integrity across the application.
/// </summary>
public class InputSanitizer : IInputSanitizer
{
    private readonly ILogger<InputSanitizer> _logger;
    
    // Common dangerous patterns to detect and block
    private static readonly Regex HtmlTagsRegex = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ScriptTagsRegex = new(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex SqlInjectionRegex = new(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ControlCharactersRegex = new(@"[\x00-\x1F\x7F-\x9F]", RegexOptions.Compiled);
    private static readonly Regex ProjectNameRegex = new(@"^[a-zA-Z0-9]([a-zA-Z0-9\-_ ]){0,62}[a-zA-Z0-9]$", RegexOptions.Compiled);
    private static readonly Regex WiqlDangerousRegex = new(@"(\b(EXEC|EXECUTE|DROP|CREATE|ALTER|TRUNCATE|xp_|sp_)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public InputSanitizer(ILogger<InputSanitizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            // Remove script tags completely
            string sanitized = ScriptTagsRegex.Replace(input, string.Empty);
            
            // Remove all HTML tags (basic sanitization - for production consider using HtmlSanitizer library)
            sanitized = HtmlTagsRegex.Replace(sanitized, string.Empty);
            
            // Remove control characters
            sanitized = ControlCharactersRegex.Replace(sanitized, string.Empty);
            
            // HTML decode to handle encoded malicious content
            sanitized = System.Net.WebUtility.HtmlDecode(sanitized);
            
            // Re-encode for safety
            sanitized = System.Net.WebUtility.HtmlEncode(sanitized);

            if (input != sanitized)
            {
                _logger.LogWarning("HTML content was sanitized. Original length: {OriginalLength}, Sanitized length: {SanitizedLength}", 
                    input.Length, sanitized.Length);
            }

            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing HTML input");
            return System.Net.WebUtility.HtmlEncode(input);
        }
    }

    public string SanitizeText(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            // Remove control characters
            string sanitized = ControlCharactersRegex.Replace(input, string.Empty);

            // Check for potential SQL injection patterns
            if (SqlInjectionRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Potential SQL injection attempt detected in text input");
                throw new ArgumentException("Input contains potentially dangerous SQL commands");
            }

            // Trim and normalize whitespace
            sanitized = sanitized.Trim();
            sanitized = Regex.Replace(sanitized, @"\s+", " ", RegexOptions.Compiled);

            return sanitized;
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing text input");
            return input.Trim(); // Return trimmed original input as fallback since we know input is not null/empty
        }
    }

    public string SanitizeWiql(string? wiql)
    {
        if (string.IsNullOrWhiteSpace(wiql))
            throw new ArgumentException("WIQL query cannot be null or empty");

        try
        {
            string sanitized = wiql.Trim();
            
            // Check for dangerous SQL commands that shouldn't be in WIQL
            if (WiqlDangerousRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Dangerous commands detected in WIQL query: {Query}", wiql);
                throw new ArgumentException("WIQL query contains prohibited commands");
            }

            // Validate WIQL starts with SELECT
            if (!sanitized.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid WIQL query format - must start with SELECT: {Query}", wiql);
                throw new ArgumentException("WIQL query must start with SELECT statement");
            }

            // Remove control characters
            sanitized = ControlCharactersRegex.Replace(sanitized, " ");
            
            // Normalize whitespace
            sanitized = Regex.Replace(sanitized, @"\s+", " ", RegexOptions.Compiled);

            return sanitized;
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing WIQL query");
            throw new ArgumentException("Invalid WIQL query format", ex);
        }
    }

    public ValidationResult ValidateInput(string? input, int maxLength = 1000, string? allowedPattern = null)
    {
        if (string.IsNullOrEmpty(input))
            return ValidationResult.Success!;

        var errors = new List<string>();

        // Length validation
        if (input.Length > maxLength)
        {
            errors.Add($"Input exceeds maximum length of {maxLength} characters");
        }

        // Pattern validation if specified
        if (!string.IsNullOrEmpty(allowedPattern))
        {
            try
            {
                var regex = new Regex(allowedPattern, RegexOptions.Compiled);
                if (!regex.IsMatch(input))
                {
                    errors.Add("Input contains invalid characters");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid regex pattern provided: {Pattern}", allowedPattern);
                errors.Add("Invalid validation pattern");
            }
        }

        // Check for control characters
        if (ControlCharactersRegex.IsMatch(input))
        {
            errors.Add("Input contains invalid control characters");
        }

        // Check for potential injection patterns
        if (SqlInjectionRegex.IsMatch(input))
        {
            errors.Add("Input contains potentially dangerous SQL commands");
        }

        return errors.Count == 0 
            ? ValidationResult.Success! 
            : new ValidationResult(string.Join("; ", errors));
    }

    public string SanitizeProjectName(string? projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name cannot be null or empty");

        try
        {
            string sanitized = projectName.Trim();

            // Validate project name format (Azure DevOps naming rules)
            if (!ProjectNameRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Invalid project name format: {ProjectName}", projectName);
                throw new ArgumentException("Project name contains invalid characters or format");
            }

            // Check length constraints (Azure DevOps limits)
            if (sanitized.Length < 1 || sanitized.Length > 64)
            {
                throw new ArgumentException("Project name must be between 1 and 64 characters");
            }

            return sanitized;
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating project name");
            throw new ArgumentException("Invalid project name format", ex);
        }
    }

    public string ValidateUrl(string? url, string[]? allowedSchemes = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty");

        allowedSchemes ??= new[] { "https" }; // Default to HTTPS only for security

        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? parsedUri))
            {
                throw new ArgumentException("Invalid URL format");
            }

            // Validate scheme
            if (!allowedSchemes.Contains(parsedUri.Scheme, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Disallowed URL scheme: {Scheme} for URL: {Url}", parsedUri.Scheme, url);
                throw new ArgumentException($"URL scheme '{parsedUri.Scheme}' is not allowed. Allowed schemes: {string.Join(", ", allowedSchemes)}");
            }

            // Additional security checks
            if (parsedUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                parsedUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                parsedUri.Host.StartsWith("192.168.", StringComparison.OrdinalIgnoreCase) ||
                parsedUri.Host.StartsWith("10.", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Potentially unsafe internal URL detected: {Url}", url);
                // For Azure DevOps, we might want to allow certain internal addresses, but log them
            }

            return parsedUri.ToString();
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating URL");
            throw new ArgumentException("Invalid URL format", ex);
        }
    }

    public string SanitizeFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty");

        try
        {
            string sanitized = filePath.Trim();

            // Check for directory traversal attempts
            if (sanitized.Contains("..") || sanitized.Contains("//") || sanitized.Contains("\\\\"))
            {
                _logger.LogWarning("Directory traversal attempt detected in file path: {FilePath}", filePath);
                throw new ArgumentException("File path contains directory traversal patterns");
            }

            // Remove control characters
            sanitized = ControlCharactersRegex.Replace(sanitized, string.Empty);

            // Check for invalid file path characters
            char[] invalidChars = Path.GetInvalidPathChars();
            if (sanitized.IndexOfAny(invalidChars) >= 0)
            {
                throw new ArgumentException("File path contains invalid characters");
            }

            // Normalize path separators
            sanitized = Path.GetFullPath(sanitized);

            return sanitized;
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing file path");
            throw new ArgumentException("Invalid file path format", ex);
        }
    }

    public string SanitizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            // Validate JSON structure
            using var document = System.Text.Json.JsonDocument.Parse(json);
            
            // Re-serialize to ensure proper formatting and remove any potential issues
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 32 // Prevent excessive nesting
            };

            return System.Text.Json.JsonSerializer.Serialize(document.RootElement, options);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON format detected");
            throw new ArgumentException("Invalid JSON format", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing JSON");
            throw new ArgumentException("JSON sanitization failed", ex);
        }
    }
}