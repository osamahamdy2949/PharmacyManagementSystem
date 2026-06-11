using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.PL.Models;
using System.Diagnostics;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class HomeController : Controller
{
    private readonly IReportService _reportService;

    public HomeController(IReportService reportService) => _reportService = reportService;

    public async Task<IActionResult> Index()
    {
        var dashboard = await _reportService.GetDashboardAsync();
        return View(dashboard);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
