using Microsoft.AspNetCore.Identity;

namespace RPSSL.IdentityService.Models
{
    public class User : IdentityUser
    {        
        public string Email { get; set; }
    }
}
