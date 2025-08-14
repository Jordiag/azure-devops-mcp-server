using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Middleware that provides request/response sanitization and validation.
/// </summary>
public class RequestSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestSanitizationMiddleware> _logger;
    private readonly IInputSanitizer _inputSanitizer;

    public RequestSanitizationMiddleware(
        RequestDelegate next,
        ILogger<RequestSanitizationMiddleware> logger,
        IInputSanitizer inputSanitizer)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputSanitizer = inputSanitizer ?? throw new ArgumentNullException(nameof(inputSanitizer));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await ValidateRequestAsync(context);

            Stream originalBodyStream = context.Response.Body;
            using MemoryStream responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            await SanitizeResponseAsync(context, originalBodyStream);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("dangerous") || ex.Message.Contains("invalid"))
        {
            _logger.LogWarning(ex, "Request blocked due to validation failure");
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsync("Invalid request data");
        }
        catch (Exception)
        {
            throw; // Re-throw to let global error handling deal with it
        }
    }

    private async Task ValidateRequestAsync(HttpContext context)
    {
        HttpRequest request = context.Request;

        ValidateHeaders(request.Headers);

        ValidateQueryParameters(request.Query);

        if (request.Method == "POST" || request.Method == "PUT")
        {
            await ValidateRequestBodyAsync(request);
        }

        if (request.ContentLength > 10 * 1024 * 1024) // 10MB limit
        {
            _logger.LogWarning("Request body too large: {ContentLength} bytes", request.ContentLength);
            throw new ArgumentException("Request body too large");
        }

        _logger.LogDebug("Request validation completed successfully");
    }

    private void ValidateHeaders(IHeaderDictionary headers)
    {
        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in headers)
        {
            foreach (string? value in header.Value)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                string sanitizedValue = _inputSanitizer.SanitizeText(value);
                if (sanitizedValue != value)
                {
                    _logger.LogWarning("Suspicious header value detected: {HeaderName}", header.Key);
                    // For security, we could reject the request here
                }

                 if (value.Length > 8192) // 8KB limit per header
                {
                    _logger.LogWarning("Header value too long: {HeaderName} ({Length} characters)", header.Key, value.Length);
                    throw new ArgumentException($"Header '{header.Key}' value too long");
                }
            }
        }
    }

    private void ValidateQueryParameters(IQueryCollection query)
    {
        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> param in query)
        {
            foreach (string? value in param.Value)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                System.ComponentModel.DataAnnotations.ValidationResult? validationResult = _inputSanitizer.ValidateInput(value, maxLength: 2000);
                if (validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    _logger.LogWarning("Invalid query parameter: {ParameterName} - {Error}", param.Key, validationResult?.ErrorMessage);
                    throw new ArgumentException($"Invalid query parameter '{param.Key}': {validationResult?.ErrorMessage}");
                }
            }
        }
    }

    private async Task ValidateRequestBodyAsync(HttpRequest request)
    {
        if (request.Body == null || !request.Body.CanRead)
            return;

        request.EnableBuffering(); // Allow reading the body multiple times

        using StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        string body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // Reset position for next middleware

        if (string.IsNullOrEmpty(body))
            return;

        if (request.ContentType?.Contains("application/json") == true)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(body);
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                throw new ArgumentException("Invalid JSON format in request body");
            }
        }

        string sanitizedBody = _inputSanitizer.SanitizeText(body);
        if (sanitizedBody != body)
        {
            _logger.LogWarning("Request body required sanitization");
            // to replace the request stream
        }
    }

    private async Task SanitizeResponseAsync(HttpContext context, Stream originalBodyStream)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        if (context.Response.ContentType?.Contains("text/") == true ||
            context.Response.ContentType?.Contains("application/json") == true)
        {
            string sanitizedResponse = _inputSanitizer.SanitizeText(responseBody);
            
            if (sanitizedResponse != responseBody)
            {
                _logger.LogInformation("Response content was sanitized");
                byte[] sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedResponse);
                context.Response.ContentLength = sanitizedBytes.Length;
                await originalBodyStream.WriteAsync(sanitizedBytes);
            }
            else
            {
                await context.Response.Body.CopyToAsync(originalBodyStream);
            }
        }
        else
        {
            await context.Response.Body.CopyToAsync(originalBodyStream);
        }

        context.Response.Body = originalBodyStream;
    }
}