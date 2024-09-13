using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RPSSL.Identity.Services;
using RPSSL.IdentityService.Models;
using System.Security.Claims;
using System.Text;

namespace RPSSL.Identity.Security
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            // Generating tokens using Microsoft.IdentityModel.JsonWebTokens;
            // https://stackoverflow.com/questions/78066474/how-to-use-the-new-microsoft-identitymodel-jsonwebtokens-to-create-a-jwtsecurity

            var data = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var securityKey = new SymmetricSecurityKey(data);

            var claims = new Dictionary<string, object>
            {
                [ClaimTypes.Name] = user.UserName,
                [ClaimTypes.Sid] = Guid.NewGuid().ToString()//"3c545f1c-cc1b-4cd5-985b-8666886f985b"
            };
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Claims = claims,
                IssuedAt = null,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JsonWebTokenHandler();
            handler.SetDefaultTimesOnTokenCreation = false;
            var tokenString = handler.CreateToken(descriptor);
            return tokenString;
        }
    }
}
