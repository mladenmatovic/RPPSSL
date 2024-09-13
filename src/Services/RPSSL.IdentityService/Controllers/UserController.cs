using Microsoft.AspNetCore.Mvc;
using RPSSL.Identity.DTOs;
using RPSSL.Identity.Services;

namespace RPSSL.Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userDto)
        {
            var result = await _userService.RegisterUserAsync(userDto);
            if (result.Succeeded)
                return Ok(new { Message = "User registered successfully" });
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var result = await _userService.LoginAsync(userDto);
            if (result.Succeeded)
                return Ok(new { Token = result.Token });
            return Unauthorized("Invalid credentials");
        }
    }
}
