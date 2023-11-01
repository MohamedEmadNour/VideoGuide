using Microsoft.AspNetCore.Identity;

namespace VideoGuide.Data
{
    public class ApplicationUser :IdentityUser
    {
        public string FullName { get; set; } = "";

    }
}
