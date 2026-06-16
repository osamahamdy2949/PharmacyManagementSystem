using Microsoft.AspNetCore.Identity;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.PL.Middlewares;

public class LoginTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public LoginTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/Account/Logout") && context.Request.Method == "POST")
        {
            var dbContext = context.RequestServices.GetRequiredService<PharmacyDbContext>();
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var now = DateTime.UtcNow;
                var activeSessions = dbContext.UserActivities
                    .Where(a => a.UserId == user.Id && a.LogoutTime == null)
                    .ToList();

                foreach (var session in activeSessions)
                {
                    session.LogoutTime = now;
                }

                if (activeSessions.Count > 0)
                    await dbContext.SaveChangesAsync();
            }

            await _next(context);
            return;
        }

        await _next(context);
    }
}
