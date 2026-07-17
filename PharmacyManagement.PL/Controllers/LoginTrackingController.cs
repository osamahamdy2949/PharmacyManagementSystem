using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class LoginTrackingController : Controller
{
    private readonly PharmacyDbContext _context;

    public LoginTrackingController(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var logs = await _context.UserActivities
            .Include(x => x.User)
            .OrderByDescending(x => x.LoginTime)
            .ToListAsync();
            
        return View(PagedList<UserActivity>.Create(logs, page));
    }
}
