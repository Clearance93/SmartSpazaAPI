using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartConfiguration;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class ExceptionConfigurationsTests
    {
        private static ExceptionConfigurations CreateMiddleware(RequestDelegate next, bool development)
        {
            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(e => e.EnvironmentName)
                       .Returns(development ? Environments.Development : Environments.Production);

            return new ExceptionConfigurations(next, NullLogger<ExceptionConfigurations>.Instance, environment.Object);
        }

        private static async Task<DefaultHttpContext> InvokeAsync(Exception exception, bool development = false)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = CreateMiddleware(_ => throw exception, development);

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return context;
        }

        [Fact]
        public async Task KeyNotFoundException_MapsTo404()
        {
            var context = await InvokeAsync(new KeyNotFoundException("missing"));

            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task ValidationException_MapsTo400()
        {
            var context = await InvokeAsync(new ValidationException("bad input"));

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task UnauthorizedException_MapsTo401()
        {
            var context = await InvokeAsync(new UnauthorizedException("nope"));

            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task ForbiddenException_MapsTo403()
        {
            var context = await InvokeAsync(new ForbiddenException("no"));

            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }

        [Fact]
        public async Task GenericException_MapsTo500_WithoutLeakingDetails()
        {
            var context = await InvokeAsync(new InvalidOperationException("secret-detail"));

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            using var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();

            Assert.DoesNotContain("secret-detail", body);
        }
    }
}
