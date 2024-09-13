using RPSSL.Identity.DTOs;

namespace RPSSL.Identity.Services
{
    public interface IUserService
    {
        Task<(bool Succeeded, string[] Errors)> RegisterUserAsync(UserRegistrationDto userDto);
        Task<(bool Succeeded, string? Token)> LoginAsync(UserLoginDto userDto);
    }
}
