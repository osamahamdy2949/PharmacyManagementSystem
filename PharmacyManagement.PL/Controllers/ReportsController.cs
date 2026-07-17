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
    private readonly IDocumentGeneratorService _docService;

    public ReportsController(
        IReportService reportService,
        ISupplierService supplierService,
        IPurchaseService purchaseService,
        InvoicePdfService pdfService,
        IDocumentGeneratorService docService)
    {
        _reportService = reportService;
        _supplierService = supplierService;
        _purchaseService = purchaseService;
        _pdfService = pdfService;
        _docService = docService;
    }

    public async Task<IActionResult> InventoryReport(int page = 1) =>
        View(PagedList<InventoryReportItemViewModel>.Create(await _reportService.GetInventoryReportAsync(), page));

    public async Task<IActionResult> LowStockReport(int page = 1) =>
        View(PagedList<PharmacyManagement.BLL.ViewModels.MedicineViewModels.MedicineViewModel>.Create(await _reportService.GetLowStockReportAsync(), page));

    public async Task<IActionResult> ExpiryReport(int page = 1) =>
        View(PagedList<PharmacyManagement.BLL.ViewModels.MedicineViewModels.MedicineViewModel>.Create(await _reportService.GetExpiryReportAsync(), page));

    public async Task<IActionResult> SalesReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null, int page = 1)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(PagedList<SalesReportItemViewModel>.Create(await _reportService.GetSalesReportAsync(period, from, to), page));
    }

    public async Task<IActionResult> PurchaseReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null, int page = 1)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(PagedList<PurchaseReportItemViewModel>.Create(await _reportService.GetPurchaseReportAsync(period, from, to), page));
    }

    public async Task<IActionResult> ProfitReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        ViewBag.Period = period;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(await _reportService.GetProfitReportAsync(period, from, to));
    }

    public async Task<IActionResult> TopSellingMedicinesReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null, int page = 1)
    {
        ViewBag.Period = period;
        return View(PagedList<TopSellingMedicineViewModel>.Create(await _reportService.GetTopSellingMedicinesReportAsync(period, from, to), page));
    }

    public async Task<IActionResult> SalesByCategoryReport(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null, int page = 1)
    {
        ViewBag.Period = period;
        return View(PagedList<SalesByCategoryViewModel>.Create(await _reportService.GetSalesByCategoryReportAsync(period, from, to), page));
    }

    public async Task<IActionResult> ExpiredMedicinesReport(int page = 1) =>
        View(PagedList<ExpiredMedicineReportItemViewModel>.Create(await _reportService.GetExpiredMedicinesReportAsync(), page));

    public async Task<IActionResult> FinishedMedicinesReport(int page = 1)
    {
        var model = new FinishedMedicinesPageViewModel
        {
            Medicines = PagedList<FinishedMedicineReportItemViewModel>.Create(await _reportService.GetFinishedMedicinesReportAsync(), page),
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

    // ========================== Export Endpoints ==========================

    public async Task<IActionResult> ExportSalesExcel(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        var sales = await _reportService.GetSalesReportAsync(period, from, to);
        var bytes = _docService.GenerateSalesExcelReport(sales);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SalesReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> ExportSalesPdf(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null)
    {
        var sales = await _reportService.GetSalesReportAsync(period, from, to);
        var bytes = _docService.GenerateSalesPdfReport(sales, $"Sales Report — {period}");
        return File(bytes, "application/pdf", $"SalesReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> ExportInventoryExcel()
    {
        var inventory = await _reportService.GetInventoryReportAsync();
        var bytes = _docService.GenerateInventoryExcelReport(inventory);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"InventoryReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> ExportInventoryPdf()
    {
        var inventory = await _reportService.GetInventoryReportAsync();
        var bytes = _docService.GenerateInventoryPdfReport(inventory, "Inventory Report");
        return File(bytes, "application/pdf", $"InventoryReport_{DateTime.Now:yyyyMMdd}.pdf");
    }
}

