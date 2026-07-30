using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.SeedingData;

public static class IdentitySeeder
{
    public static readonly string[] Roles =
    [
        RoleNames.Administrator,
        RoleNames.Pharmacist,
        RoleNames.Cashier
    ];

    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting Identity seeding...");

            foreach (var role in Roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    logger.LogInformation("Role '{Role}' already exists.", role);
                    continue;
                }

                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        logger.LogError(
                            "Failed to create role '{Role}'. {Code}: {Description}",
                            role,
                            error.Code,
                            error.Description);
                    }

                    throw new Exception($"Failed to create role '{role}'.");
                }

                logger.LogInformation("Role '{Role}' created successfully.", role);
            }

            await EnsureUserAsync(userManager, logger,
                "admin@pharmacy.com",
                "Admin@123",
                "System Administrator",
                RoleNames.Administrator);

            await EnsureUserAsync(userManager, logger,
                "pharmacist@pharmacy.com",
                "Pharma@123",
                "Default Pharmacist",
                RoleNames.Pharmacist);

            await EnsureUserAsync(userManager, logger,
                "cashier@pharmacy.com",
                "Cashier@123",
                "Default Cashier",
                RoleNames.Cashier);

            logger.LogInformation("Identity seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity.");
            throw;
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string password,
        string fullName,
        string role)
    {
        logger.LogInformation("Checking user '{Email}'...", email);

        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
        {
            logger.LogInformation("User '{Email}' already exists.", email);
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        logger.LogInformation("Creating user '{Email}'...", email);

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                logger.LogError(
                    "Failed to create user '{Email}'. {Code}: {Description}",
                    email,
                    error.Code,
                    error.Description);
            }

            throw new Exception($"Failed to create user '{email}'.");
        }

        logger.LogInformation("User '{Email}' created successfully.", email);

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                logger.LogError(
                    "Failed to add user '{Email}' to role '{Role}'. {Code}: {Description}",
                    email,
                    role,
                    error.Code,
                    error.Description);
            }

            throw new Exception($"Failed to add '{email}' to role '{role}'.");
        }

        logger.LogInformation(
            "User '{Email}' added to role '{Role}' successfully.",
            email,
            role);
    }
}