using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PurchaseReturnViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class PurchaseReturnController : Controller
{
    private readonly IPurchaseReturnService _service;

    public PurchaseReturnController(IPurchaseReturnService service) => _service = service;

    public async Task<IActionResult> Index(int page = 1) =>
        View(PagedList<PurchaseReturnListItemViewModel>.Create(await _service.GetAllAsync(), page));

    public async Task<IActionResult> Details(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Create(int? invoiceId)
    {
        if (!invoiceId.HasValue)
            return RedirectToAction("Index", "PurchaseInvoice");

        var model = await _service.GetCreateModelFromInvoiceAsync(invoiceId.Value);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseReturnViewModel model)
    {
        var result = await _service.CreateAsync(model);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = $"Purchase return #{result.Data} created.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }
}
