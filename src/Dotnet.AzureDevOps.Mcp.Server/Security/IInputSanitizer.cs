using System.ComponentModel.DataAnnotations;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Provides input sanitization and validation services to prevent injection attacks
/// and ensure data integrity across the application.
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// Sanitizes HTML content by removing potentially dangerous elements and attributes.
    /// </summary>
    /// <param name="input">The HTML content to sanitize</param>
    /// <returns>Sanitized HTML content safe for display</returns>
    string SanitizeHtml(string? input);

    /// <summary>
    /// Sanitizes general text input by removing control characters and potentially dangerous content.
    /// </summary>
    /// <param name="input">The text input to sanitize</param>
    /// <returns>Sanitized text content</returns>
    string SanitizeText(string? input);

    /// <summary>
    /// Validates and sanitizes WIQL queries to prevent injection attacks.
    /// </summary>
    /// <param name="wiql">The WIQL query to validate and sanitize</param>
    /// <returns>Sanitized WIQL query</returns>
    /// <exception cref="ArgumentException">Thrown when WIQL contains dangerous content</exception>
    string SanitizeWiql(string? wiql);

    /// <summary>
    /// Validates input against common patterns and length constraints.
    /// </summary>
    /// <param name="input">The input to validate</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <param name="allowedPattern">Regex pattern for allowed characters</param>
    /// <returns>Validation results</returns>
    ValidationResult ValidateInput(string? input, int maxLength = 1000, string? allowedPattern = null);

    /// <summary>
    /// Sanitizes project names to ensure they contain only valid characters.
    /// </summary>
    /// <param name="projectName">The project name to sanitize</param>
    /// <returns>Sanitized project name</returns>
    /// <exception cref="ArgumentException">Thrown when project name is invalid</exception>
    string SanitizeProjectName(string? projectName);

    /// <summary>
    /// Validates URL inputs to ensure they are properly formatted and safe.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <param name="allowedSchemes">Allowed URL schemes (default: https only)</param>
    /// <returns>Validated URL</returns>
    /// <exception cref="ArgumentException">Thrown when URL is invalid or unsafe</exception>
    string ValidateUrl(string? url, string[]? allowedSchemes = null);

    /// <summary>
    /// Sanitizes file paths to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to sanitize</param>
    /// <returns>Sanitized file path</returns>
    /// <exception cref="ArgumentException">Thrown when file path is invalid or unsafe</exception>
    string SanitizeFilePath(string? filePath);

    /// <summary>
    /// Sanitizes JSON content to ensure it's well-formed and safe.
    /// </summary>
    /// <param name="json">The JSON to sanitize</param>
    /// <returns>Sanitized JSON</returns>
    /// <exception cref="ArgumentException">Thrown when JSON is malformed</exception>
    string SanitizeJson(string? json);
}