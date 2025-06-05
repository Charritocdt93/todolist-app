namespace TodoListApp.Api.Middleware
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Extensión para registrar el ExceptionHandlingMiddleware en la tubería HTTP.
        /// </summary>
        public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
