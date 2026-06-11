using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class SupplierController : Controller
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public IActionResult Create() => View(new SupplierViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierViewModel model)
    {
        var result = await _service.CreateAsync(model);
        if (!result.Success) { ModelState.AddServiceErrors(result); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SupplierViewModel model)
    {
        var result = await _service.UpdateAsync(model);
        if (!result.Success) { ModelState.AddServiceErrors(result); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
