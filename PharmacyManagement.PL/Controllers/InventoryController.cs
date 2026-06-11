using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.DAL.Common;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class InventoryController : Controller
{
    private readonly IReportService _reportService;

    public InventoryController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index(InventoryFilter filter = InventoryFilter.All)
    {
        ViewBag.Filter = filter;
        return View(await _reportService.GetBatchInventoryAsync(filter));
    }
}
