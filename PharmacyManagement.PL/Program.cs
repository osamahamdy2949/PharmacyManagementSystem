using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using PharmacyManagement.BLL;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.DAL;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Services;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.PL;

public class Program
{
    public static async Task Main(string[] args)
    {
        var displayCulture = CurrencyHelper.DisplayCultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = displayCulture;
        CultureInfo.DefaultThreadCurrentUICulture = displayCulture;

        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<CurrentUserService>();
        builder.Services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());
        builder.Services.AddScoped<IAuditUserProvider>(sp => sp.GetRequiredService<CurrentUserService>());

        builder.Services.AddDataAccess(connectionString);

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<PharmacyDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddBusinessLogic();
        builder.Services.AddScoped<InvoicePdfService>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        var app = builder.Build();

        await app.MigrateAndSeedDarabaseAsync();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<PharmacyManagement.PL.Middlewares.LoginTrackingMiddleware>();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        await app.RunAsync();
    }
}
