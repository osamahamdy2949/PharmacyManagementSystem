using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.RestockViewModels;
using PharmacyManagement.PL.Helpers;
using PharmacyManagement.PL.Services;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ISupplierService _supplierService;
    private readonly IPurchaseService _purchaseService;
    private readonly InvoicePdfService _pdfService;

    public ReportsController(
        IReportService reportService,
        ISupplierService supplierService,
        IPurchaseService purchaseService,
        InvoicePdfService pdfService)
    {
        _reportService = reportService;
        _supplierService = supplierService;
        _purchaseService = purchaseService;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> InventoryReport() =>
        View(await _reportService.GetInventoryReportAsync());

    public async Task<IActionResult> LowStockReport() =>
        View(await _reportService.GetLowStockReportAsync());

    public async Task<IActionResult> ExpiryReport() =>
        View(await _reportService.GetExpiryReportAsync());

    public async Task<IActionResult> SalesReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(await _reportService.GetSalesReportAsync(period, from, to));
    }

    public async Task<IActionResult> PurchaseReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(await _reportService.GetPurchaseReportAsync(period, from, to));
    }

    public async Task<IActionResult> ProfitReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(await _reportService.GetProfitReportAsync(period, from, to));
    }

    public async Task<IActionResult> TopSellingMedicinesReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        return View(await _reportService.GetTopSellingMedicinesReportAsync(period, from, to));
    }

    public async Task<IActionResult> SalesByCategoryReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        return View(await _reportService.GetSalesByCategoryReportAsync(period, from, to));
    }

    public async Task<IActionResult> ExpiredMedicinesReport() =>
        View(await _reportService.GetExpiredMedicinesReportAsync());

    public async Task<IActionResult> FinishedMedicinesReport()
    {
        var model = new FinishedMedicinesPageViewModel
        {
            Medicines = await _reportService.GetFinishedMedicinesReportAsync(),
            Suppliers = await _supplierService.GetAllAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestockFinishedMedicine(RestockFinishedMedicineRequest request)
    {
        var result = await _purchaseService.RestockFinishedMedicineAsync(request);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(FinishedMedicinesReport));
        }

        TempData["Success"] = $"Restocked successfully. Purchase invoice #{result.Data} created.";
        TempData["RestockInvoiceId"] = result.Data;
        return RedirectToAction(nameof(FinishedMedicinesReport));
    }

    public async Task<IActionResult> DownloadFinishedMedicinesPdf()
    {
        var pdf = await _pdfService.GenerateFinishedMedicinesReportAsync();
        return pdf == null ? NotFound() : File(pdf, "application/pdf", $"FinishedMedicines_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> DownloadRestockReceipt(int id)
    {
        var pdf = await _pdfService.GeneratePurchaseReceiptAsync(id);
        return pdf == null ? NotFound() : File(pdf, "application/pdf", $"RestockReceipt_{id}.pdf");
    }
}
