using Microsoft.AspNetCore.Identity;

namespace MangoFusionApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}
