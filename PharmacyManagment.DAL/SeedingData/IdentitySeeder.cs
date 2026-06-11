using Microsoft.AspNetCore.Identity;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.SeedingData;

public static class IdentitySeeder
{
    public static readonly string[] Roles = [RoleNames.Administrator, RoleNames.Pharmacist, RoleNames.Cashier];

    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "admin@pharmacy.com", "Admin@123", "System Administrator", RoleNames.Administrator);
        await EnsureUserAsync(userManager, "pharmacist@pharmacy.com", "Pharma@123", "Default Pharmacist", RoleNames.Pharmacist);
        await EnsureUserAsync(userManager, "cashier@pharmacy.com", "Cashier@123", "Default Cashier", RoleNames.Cashier);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) return;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}
