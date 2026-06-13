using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;
using PharmacyManagement.PL.Services;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class PurchaseInvoiceController : Controller
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISupplierService _supplierService;
    private readonly IMedicineService _medicineService;
    private readonly IValidator<CreatePurchaseInvoiceViewModel> _createValidator;
    private readonly InvoicePdfService _pdfService;
    private readonly IShiftService _shiftService;

    public PurchaseInvoiceController(
        IPurchaseService purchaseService,
        ISupplierService supplierService,
        IMedicineService medicineService,
        IValidator<CreatePurchaseInvoiceViewModel> createValidator,
        InvoicePdfService pdfService,
        IShiftService shiftService)
    {
        _purchaseService = purchaseService;
        _supplierService = supplierService;
        _medicineService = medicineService;
        _createValidator = createValidator;
        _pdfService = pdfService;
        _shiftService = shiftService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.PendingBatches = await _purchaseService.GetPendingBatchesAsync();
        return View(await _purchaseService.GetAllAsync());
    }

    public async Task<IActionResult> Create()
    {
        var shift = await _shiftService.GetActiveShiftAsync();
        if (shift == null)
        {
            TempData["Error"] = "You must start a shift before recording purchases.";
            return RedirectToAction("Current", "Shift");
        }

        await PopulateCreateLookupsAsync();
        var model = new CreatePurchaseInvoiceViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseInvoiceViewModel model)
    {
        foreach (var item in model.Items)
            item.BuildDose();

        var validation = await _createValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await PopulateCreateLookupsAsync();
            return View(model);
        }

        var result = await _purchaseService.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create purchase invoice.");
            await PopulateCreateLookupsAsync();
            return View(model);
        }

        TempData["Success"] = $"Purchase invoice #{result.Data} created successfully. Stock is pending activation.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    private async Task PopulateCreateLookupsAsync()
    {
        var suppliers = await _supplierService.GetAllAsync();
        var medicines = await _medicineService.GetAllAsync();
        ViewBag.SupplierId = new SelectList(suppliers, "Id", "Name");
        ViewBag.Medicines = new SelectList(medicines, "Id", "TradeName");
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _purchaseService.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> DownloadReceipt(int id)
    {
        var pdf = await _pdfService.GeneratePurchaseReceiptAsync(id);
        return pdf == null ? NotFound() : File(pdf, "application/pdf", $"PurchaseReceipt_{id}.pdf");
    }

    public async Task<IActionResult> Activate(int id)
    {
        var model = await _purchaseService.GetActivateBatchModelAsync(id);
        if (model == null) return NotFound();
        await LoadActivationLookupsAsync();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ActivateBatchViewModel model)
    {
        var result = await _purchaseService.ActivateBatchAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Activation failed.");
            await LoadActivationLookupsAsync();
            return View(model);
        }

        TempData["Success"] = "Stock activated and moved to inventory.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPendingBatchQuantity(int id, int quantity)
    {
        var result = await _purchaseService.EditPendingBatchQuantityAsync(id, quantity);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage ?? "Failed to update pending batch quantity.";
        }
        else
        {
            TempData["Success"] = "Pending batch quantity updated successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePendingBatch(int id)
    {
        var result = await _purchaseService.DeletePendingBatchAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete pending batch.";
        }
        else
        {
            TempData["Success"] = "Pending batch deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadActivationLookupsAsync()
    {
        ViewBag.PurchaseUnits = new SelectList(Enum.GetValues(typeof(PharmacyManagement.DAL.Data.Entities.Enums.PurchaseUnit))
            .Cast<PharmacyManagement.DAL.Data.Entities.Enums.PurchaseUnit>()
            .Select(u => new { Id = (int)u, Name = u.ToString() }), "Id", "Name");
        ViewBag.SaleUnits = new SelectList(Enum.GetValues(typeof(PharmacyManagement.DAL.Data.Entities.Enums.SaleUnit))
            .Cast<PharmacyManagement.DAL.Data.Entities.Enums.SaleUnit>()
            .Select(u => new { Id = (int)u, Name = u.ToString() }), "Id", "Name");
    }
}
