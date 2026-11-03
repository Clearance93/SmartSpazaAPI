namespace SmartSpazaAPI.Middleware
{
    /// <summary>
    /// Adds conservative security headers to every response. API responses get a locked-down
    /// policy; the hosted Angular app gets a policy that allows its own bundle and inline styles,
    /// plus camera access for the barcode scanner.
    /// </summary>
    public sealed class SecurityHeadersMiddleware
    {
        private const string StrictApiPolicy =
            "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

        private const string SwaggerPolicy =
            "default-src 'self'; img-src 'self' data:; font-src 'self' data:; " +
            "style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'";

        private const string SpaPolicy =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "font-src 'self' data:; " +
            "img-src 'self' data: blob:; " +
            "media-src 'self' blob:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Apply(context);

            // Re-apply when the response starts: exception middleware clears the response before
            // writing an error body, and this callback survives that reset.
            context.Response.OnStarting(() =>
            {
                Apply(context);
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private static void Apply(HttpContext context)
        {
            var headers = context.Response.Headers;
            var path = context.Request.Path;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";

            if (path.StartsWithSegments("/swagger"))
            {
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                headers["Content-Security-Policy"] = SwaggerPolicy;
            }
            else if (path.StartsWithSegments("/api") || path.StartsWithSegments("/health"))
            {
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                headers["Content-Security-Policy"] = StrictApiPolicy;
            }
            else
            {
                // The hosted Angular app: the scanner needs the camera.
                headers["Permissions-Policy"] = "camera=(self), microphone=(), geolocation=(self)";
                headers["Content-Security-Policy"] = SpaPolicy;
            }

            if (path.StartsWithSegments("/api/Auth"))
            {
                // Authentication responses contain tokens; never cache them.
                headers["Cache-Control"] = "no-store";
            }
        }
    }
}
