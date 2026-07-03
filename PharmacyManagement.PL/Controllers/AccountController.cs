using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.PL.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PharmacyDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        PharmacyDbContext dbContext,
        INotificationService notificationService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignInAsync(user, isPersistent: false);
                await CloseActiveSessionsAsync(user.Id);
                await RecordLoginAsync(user);
                await _notificationService.GenerateStockAlertsAsync();
                return RedirectToLocal(returnUrl);
            }
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
            await CloseActiveSessionsAsync(user.Id);

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    private async Task CloseActiveSessionsAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var activeSessions = await _dbContext.UserActivities
            .Where(a => a.UserId == userId && a.LogoutTime == null)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.LogoutTime = now;
        }

        if (activeSessions.Count > 0)
            await _dbContext.SaveChangesAsync();
    }

    private async Task RecordLoginAsync(ApplicationUser user)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        _dbContext.UserActivities.Add(new UserActivity
        {
            UserId = user.Id,
            IPAddress = ip,
            UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent,
            LoginTime = DateTime.UtcNow,
            IsSuccess = true
        });

        await _dbContext.SaveChangesAsync();
    }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
