using Microsoft.AspNetCore.Identity;

namespace Costify.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public int BusinessId { get; set; } = 1;
}
