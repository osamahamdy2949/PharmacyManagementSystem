using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.SeedingData;

namespace PharmacyManagement.PL
{
    public static class ProgramExtentions
    {
        public static async Task MigrateAndSeedDarabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbcontext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var pendingMigrations = dbcontext.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Appling {pendingMigrations.Count()} Pending Migartion");
                await dbcontext.Database.MigrateAsync();
            }

            await PharmacyDbSeeder.SeedAsync(dbcontext,logger);

            await IdentitySeeder.SeedAsync(userManager,roleManager,logger);
        }
    }
}
