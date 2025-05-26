using Microsoft.AspNetCore.Http;
using FluentValidation;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Test.Models.Dto; // Assuming PostDto is here
using Microsoft.Extensions.Logging; // For logging

namespace Test.Utils
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider) // Inject IServiceProvider to resolve validator
        {
            // Only validate POST and PUT requests for /api/posts
            if ((context.Request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Method.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase)) &&
                context.Request.Path.StartsWithSegments("/api/posts", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("ValidationMiddleware: Intercepted {Method} request to {Path}", context.Request.Method, context.Request.Path);

                context.Request.EnableBuffering(); // IMPORTANT: Enable buffering

                string requestBodyText;
                using (var reader = new StreamReader(context.Request.Body, leaveOpen: true)) // leaveOpen: true so stream can be reset
                {
                    requestBodyText = await reader.ReadToEndAsync();
                    context.Request.Body.Seek(0, SeekOrigin.Begin); // IMPORTANT: Reset stream position
                }

                if (string.IsNullOrWhiteSpace(requestBodyText))
                {
                    _logger.LogWarning("ValidationMiddleware: Request body is empty for {Method} {Path}", context.Request.Method, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Request body cannot be empty.");
                    return;
                }

                PostDto? dtoToValidate = null; // Assuming PostDto is your target DTO
                try
                {
                    // Ensure JsonSerializerOptions are provided if needed (e.g., case insensitivity)
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    dtoToValidate = JsonSerializer.Deserialize<PostDto>(requestBodyText, options);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "ValidationMiddleware: Invalid JSON format for {Method} {Path}", context.Request.Method, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid JSON format in request body.");
                    return;
                }

                if (dtoToValidate == null)
                {
                    _logger.LogWarning("ValidationMiddleware: Failed to deserialize request body for {Method} {Path}", context.Request.Method, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Unable to deserialize request body.");
                    return;
                }

                // Resolve the validator dynamically for the DTO type
                // This makes the middleware more generic if you have multiple DTOs
                var validator = serviceProvider.GetService(typeof(IValidator<PostDto>)) as IValidator<PostDto>;

                if (validator == null)
                {
                    _logger.LogError("ValidationMiddleware: Validator for {DtoType} not found. Ensure it's registered.", typeof(PostDto).Name);
                    // Allow request to proceed if no validator is found, or handle as an error
                    await _next(context);
                    return;
                }

                var validationResult = await validator.ValidateAsync(dtoToValidate);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("ValidationMiddleware: Validation failed for {Method} {Path}. Errors: {@ValidationErrors}",
                        context.Request.Method, context.Request.Path, validationResult.Errors);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(validationResult.ToDictionary()); // FluentValidation.Results.ValidationResult.ToDictionary()
                    return;
                }
                _logger.LogInformation("ValidationMiddleware: Request body validated successfully for {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            await _next(context);
        }
    }
}