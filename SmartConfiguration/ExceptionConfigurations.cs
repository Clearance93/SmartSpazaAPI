using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartConfiguration
{
    public class ExceptionConfigurations
    {
        private readonly RequestDelegate _Next;
        private readonly ILogger<ExceptionConfigurations> _Logger;
        private readonly IHostEnvironment _Environment;

        public ExceptionConfigurations(RequestDelegate next,
                                       ILogger<ExceptionConfigurations> logger,
                                       IHostEnvironment environment)
        {
            _Next = next ?? throw new ArgumentNullException(nameof(next));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _Next(context);
            }
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = Map(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _Logger.LogError(exception,
                                 "An unhandled exception occurred while processing {Method} {Path}.",
                                 context.Request.Method,
                                 context.Request.Path);
            }
            else
            {
                _Logger.LogWarning(exception,
                                   "A handled exception occurred while processing {Method} {Path}.",
                                   context.Request.Method,
                                   context.Request.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            if (statusCode >= StatusCodes.Status500InternalServerError && _Environment.IsDevelopment())
            {
                await context.Response.WriteAsJsonAsync(new { Message = message, Details = exception.Message });
                return;
            }

            await context.Response.WriteAsJsonAsync(new { Message = message });
        }

        private static (int StatusCode, string Message) Map(Exception exception)
        {
            return exception switch
            {
                BaseException baseException => (baseException.StatusCode, baseException.Message),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "You are not authorized to perform this action."),
                ArgumentNullException or ArgumentException => (StatusCodes.Status400BadRequest, "The request is invalid."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
            };
        }
    }
}
