namespace UserService.API.Middleware
{
    public class MiddlewareException
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareException> _logger;

        public MiddlewareException(RequestDelegate next, ILogger<MiddlewareException> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occured {ex}", ex);

                httpContext.Response.StatusCode = 500;

                await httpContext.Response.WriteAsync("An unexpected error occured. Please try again later");
            }
        }
    }
}
