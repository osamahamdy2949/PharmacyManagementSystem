using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class CustomerController : Controller
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service) => _service = service;

    private bool CanManageCustomers =>
        User.IsInRole(RoleNames.Administrator) || User.IsInRole(RoleNames.Pharmacist);

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public IActionResult Create() => View(new CustomerViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
    {
        var result = await _service.CreateAsync(model);
        if (!result.Success) { ModelState.AddServiceErrors(result); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null) return NotFound();
        model.CanEditPhone = true;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Edit(CustomerViewModel model)
    {
        model.CanEditPhone = true;
        var result = await _service.UpdateAsync(model, CanManageCustomers);
        if (!result.Success) { ModelState.AddServiceErrors(result); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!CanManageCustomers)
            return Forbid();

        var result = await _service.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
