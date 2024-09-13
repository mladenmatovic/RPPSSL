using Microsoft.AspNetCore.Identity;
using RPSSL.Identity.DTOs;
using RPSSL.Identity.Security;
using RPSSL.IdentityService.Models;

namespace RPSSL.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtTokenGenerator jwtTokenGenerator,
            ILogger<UserService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _logger = logger;   
        }

        public async Task<(bool Succeeded, string[] Errors)> RegisterUserAsync(UserRegistrationDto userDto)
        {
            _logger.LogInformation($"Attempting to login user: {userDto.Username}", userDto.Username);

            var user = new User
            {
                UserName = userDto.Username,
                Email = userDto.Email
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                return (true, Array.Empty<string>());
            }

            _logger.LogWarning("User registration failed for {Username}. Errors: {Errors}",
                userDto.Username, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string? Token)> LoginAsync(UserLoginDto userDto)
        {
            _logger.LogInformation($"Attempting to login user: {userDto.Username}", userDto.Username);

            var user = await _userManager.FindByNameAsync(userDto.Username);
            if (user == null)
            {
                return (false, null);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
            if (result.Succeeded)
            {
                var token = _jwtTokenGenerator.GenerateJwtToken(user);
                return (true, token);
            }

            _logger.LogWarning($"Attempted login failed for user: {userDto.Username}");

            return (false, null);
        }
    }
}
