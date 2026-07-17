using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.DAL.Data.Entities.Enums;
using System;
using System.Linq;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class MedicineController : Controller
{
    private readonly IMedicineService _medicineService;
    private readonly ICategoryService _categoryService;

    public MedicineController(IMedicineService medicineService, ICategoryService categoryService)
    {
        _medicineService = medicineService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1) =>
        View(PagedList<MedicineViewModel>.Create(await _medicineService.GetAllAsync(search), page));

    public async Task<IActionResult> Details(int id)
    {
        var model = await _medicineService.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new MedicineViewModel());
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineViewModel model)
    {
        var result = await _medicineService.CreateAsync(model);
        if (!result.Success) { ModelState.AddServiceErrors(result); await LoadCategoriesAsync(); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _medicineService.GetByIdAsync(id);
        if (model == null) return NotFound();
        await LoadCategoriesAsync();
        return View(model);
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicineViewModel model)
    {
        var result = await _medicineService.UpdateAsync(model);
        if (!result.Success) { ModelState.AddServiceErrors(result); await LoadCategoriesAsync(); return View(model); }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _medicineService.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [Authorize(Roles = RoleNames.AdminOrPharmacist)]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _medicineService.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
        ViewBag.MedicineForms = new SelectList(Enum.GetValues(typeof(MedicineForm)).Cast<MedicineForm>().Select(m => new { Id = (int)m, Name = m.ToString() }), "Id", "Name");
        ViewBag.PurchaseUnits = new SelectList(Enum.GetValues(typeof(PurchaseUnit)).Cast<PurchaseUnit>().Select(u => new { Id = (int)u, Name = u.ToString() }), "Id", "Name");
        ViewBag.SaleUnits = new SelectList(Enum.GetValues(typeof(SaleUnit)).Cast<SaleUnit>().Select(u => new { Id = (int)u, Name = u.ToString() }), "Id", "Name");
    }
}
