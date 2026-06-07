using Microsoft.AspNetCore.Mvc;
using SmartDto;
using SmartInterfaces.IService;

namespace SmartSpazaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _UserService;

        public AuthController(IUserService userService)
        {
            _UserService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var result = await _UserService.AuthenticateUserAsync(userLoginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            return Ok(result);
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var result = await _UserService.CreateUserAsync(userDto);
            if (result == null)
            {
                return BadRequest(new { message = "Registration failed. Please check the provided information." });
            }

            return Ok(result);
        }
    }
}
