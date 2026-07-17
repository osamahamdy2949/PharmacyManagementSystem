using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class InventoryController : Controller
{
    private readonly IReportService _reportService;

    public InventoryController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index(InventoryFilter filter = InventoryFilter.All, int page = 1)
    {
        ViewBag.Filter = filter;
        return View(PagedList<BatchInventoryItemViewModel>.Create(await _reportService.GetBatchInventoryAsync(filter), page));
    }
}
