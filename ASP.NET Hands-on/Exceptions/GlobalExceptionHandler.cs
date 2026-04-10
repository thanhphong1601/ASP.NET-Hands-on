using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Hands_on.Exceptions
{
    // Generic global exception handling middleware
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionHandler");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            // Map some common exceptions to http status codes
            if (exception is KeyNotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is ArgumentException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                // client cancelled
                statusCode = 499; // Client Closed Request (non-standard)
                message = "Request was cancelled.";
            }

            var payload = JsonSerializer.Serialize(new { error = message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(payload);
        }
    }

    public static class ExceptionHandlerExtensions
    {
        // Register the exception handling middleware in the pipeline
        public static WebApplication AddExceptionHandler<T>(this WebApplication app) where T : class
        {
            app.UseMiddleware<T>();
            return app;
        }
    }
}
