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

    public async Task InvokeAsync(HttpContext context, PharmacyDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        if (context.Request.Path.StartsWithSegments("/Account/Logout") && context.Request.Method == "POST")
        {
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
        }
        else
        {
            await _next(context);
        }
    }
}
