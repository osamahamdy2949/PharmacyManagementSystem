using Microsoft.AspNetCore.Identity;

namespace PharmacyManagement.DAL.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
