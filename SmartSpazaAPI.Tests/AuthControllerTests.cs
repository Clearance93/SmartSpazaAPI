using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartDto;
using SmartInterfaces.IService;
using SmartSpazaAPI.Controllers;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task ChangePassword_UsesAuthenticatedUserId()
        {
            var userService = new Mock<IUserService>();
            var controller = CreateController(userService, "user-1");

            await controller.ChangePassword(new PasswordChangeDto
            {
                OldPassword = "Old_pass1",
                NewPassword = "New_pass1"
            });

            userService.Verify(service => service.ChangePasswordAsync(
                "user-1",
                It.Is<PasswordChangeDto>(dto => dto.NewPassword == "New_pass1")), Times.Once);
        }

        private static AuthController CreateController(Mock<IUserService> userService, string? userId)
        {
            var claims = userId is null
                ? new List<Claim>()
                : new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            };

            return new AuthController(userService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };
        }
    }
}
