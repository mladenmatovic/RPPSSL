using RPSSL.RandomNumberService.Exceptions;
using System.Net;
using System.Text.Json;

namespace RPSSL.RandomNumberService.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            string? result;
            switch (exception)
            {
                case RandomNumberServiceException serviceEx:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result = JsonSerializer.Serialize(new { error = "A service error occurred.", details = serviceEx.Message });
                    break;
                case RandomNumberApiException apiEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                    result = JsonSerializer.Serialize(new { error = "Error communicating with external service.", details = apiEx.Message });
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result = JsonSerializer.Serialize(new { error = "An error occurred. Please try again later." });
                    break;
            }

            return context.Response.WriteAsync(result);
        }
    }
}
