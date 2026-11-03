using Microsoft.AspNetCore.Http;
using SmartSpazaAPI.Middleware;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class HttpsRequiredMiddlewareTests
    {
        [Fact]
        public async Task PlainHttp_IsRejectedWith426()
        {
            var nextCalled = false;
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";

            var middleware = new HttpsRequiredMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            Assert.False(nextCalled);
            Assert.Equal(StatusCodes.Status426UpgradeRequired, context.Response.StatusCode);
        }

        [Fact]
        public async Task Https_IsAllowed()
        {
            var nextCalled = false;
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";

            var middleware = new HttpsRequiredMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            Assert.True(nextCalled);
        }

        [Fact]
        public async Task HealthProbe_IsAllowedOverHttp()
        {
            var nextCalled = false;
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Path = "/health";

            var middleware = new HttpsRequiredMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            Assert.True(nextCalled);
        }
    }
}
