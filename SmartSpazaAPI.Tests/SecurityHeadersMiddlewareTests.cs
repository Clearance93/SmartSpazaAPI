using Microsoft.AspNetCore.Http;
using SmartSpazaAPI.Middleware;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class SecurityHeadersMiddlewareTests
    {
        [Fact]
        public async Task AuthRequest_SetsSecurityHeadersAndNoStore()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/Auth/login";

            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
            Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
            Assert.Equal("no-store", context.Response.Headers["Cache-Control"]);
            Assert.Contains("default-src 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
        }

        [Fact]
        public async Task NonAuthRequest_SetsSecurityHeadersWithoutNoStore()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/Products/all-products";

            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
            Assert.False(context.Response.Headers.ContainsKey("Cache-Control"));
        }

        [Fact]
        public async Task SwaggerRequest_GetsARelaxedSameOriginCsp()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/swagger/index.html";

            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            var csp = context.Response.Headers["Content-Security-Policy"].ToString();
            Assert.Contains("script-src 'self'", csp);
            Assert.DoesNotContain("default-src 'none'", csp);
        }
    }
}
