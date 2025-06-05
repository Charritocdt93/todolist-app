using System.Net;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Middleware
{
    /// <summary>
    /// Middleware que captura todas las excepciones no manejadas,
    /// las traduce a respuestas HTTP y las registra con Serilog/ILogger.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Invoca el siguiente middleware en la tubería
                await _next(context);
            }
            catch (DomainException dex)
            {
                // Capturamos errores de dominio y devolvemos 400 Bad Request
                _logger.LogWarning(dex, "⚠️ DomainException capturada en request {Method} {Path}",
                                    context.Request.Method, context.Request.Path);
                await HandleDomainExceptionAsync(context, dex.Message);
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción => 500 Internal Server Error
                _logger.LogError(ex, "❌ Excepción inesperada en request {Method} {Path}",
                                  context.Request.Method, context.Request.Path);
                await HandleUnexpectedExceptionAsync(context);
            }
        }

        private static Task HandleDomainExceptionAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = message
            });

            return context.Response.WriteAsync(result);
        }

        private static Task HandleUnexpectedExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Error interno del servidor"
            });

            return context.Response.WriteAsync(result);
        }
    }
}
