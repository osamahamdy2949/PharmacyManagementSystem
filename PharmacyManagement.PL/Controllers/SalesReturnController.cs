using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.SalesReturnViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class SalesReturnController : Controller
{
    private readonly ISalesReturnService _salesReturnService;

    public SalesReturnController(ISalesReturnService salesReturnService)
    {
        _salesReturnService = salesReturnService;
    }

    public async Task<IActionResult> Index() => View(await _salesReturnService.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await _salesReturnService.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Create(int? invoiceId)
    {
        if (!invoiceId.HasValue)
            return RedirectToAction("Index", "SalesInvoice");

        var model = await _salesReturnService.GetCreateModelFromInvoiceAsync(invoiceId.Value);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesReturnViewModel model)
    {
        var result = await _salesReturnService.CreateAsync(model);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = $"Sales return #{result.Data} created successfully.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }
}
