namespace SmartSpazaAPI.Middleware
{
    /// <summary>
    /// Fail-closed HTTPS enforcement for production. This runs after forwarded headers, so a
    /// trusted reverse proxy can terminate TLS and set X-Forwarded-Proto without the API rejecting
    /// legitimate traffic. Direct plain-HTTP requests receive 426 and are not processed.
    /// </summary>
    public sealed class HttpsRequiredMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpsRequiredMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // The container health probe is internal, non-sensitive and cannot present a
            // certificate; everything else must be HTTPS in production.
            if (!context.Request.IsHttps && !context.Request.Path.StartsWithSegments("/health"))
            {
                context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "HTTPS is required. Send this request over TLS or through the trusted reverse proxy."
                });

                return;
            }

            await _next(context);
        }
    }
}
