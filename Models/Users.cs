using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
    }
}