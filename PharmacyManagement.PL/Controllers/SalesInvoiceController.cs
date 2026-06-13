using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PosViewModels;
using PharmacyManagement.PL.Helpers;
using PharmacyManagement.PL.Services;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class SalesInvoiceController : Controller
{
    private readonly ISalesService _salesService;
    private readonly ICustomerService _customerService;
    private readonly InvoicePdfService _pdfService;
    private readonly IShiftService _shiftService;

    public SalesInvoiceController(
        ISalesService salesService,
        ICustomerService customerService,
        InvoicePdfService pdfService,
        IShiftService shiftService)
    {
        _salesService = salesService;
        _customerService = customerService;
        _pdfService = pdfService;
        _shiftService = shiftService;
    }

    public async Task<IActionResult> Index([FromQuery] string? filter)
    {
        var invoices = (await _salesService.GetAllAsync()).ToList();
        
        if (!string.IsNullOrEmpty(filter))
        {
            if (filter.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                invoices = invoices.Where(i => i.SaleType == "Cash").ToList();
            else if (filter.Equals("Partial", StringComparison.OrdinalIgnoreCase))
                invoices = invoices.Where(i => i.PaymentStatus == "PartiallyPaid" || (i.SaleType == "Credit" && i.PaymentStatus == "Unpaid")).ToList();
            
            ViewBag.CurrentFilter = filter;
        }

        return View(invoices);
    }

    public async Task<IActionResult> Pos()
    {
        var shift = await _shiftService.GetActiveShiftAsync();
        if (shift == null)
        {
            TempData["Error"] = "You must start a shift before accessing POS.";
            return RedirectToAction("Current", "Shift");
        }

        var customers = await _customerService.GetAllAsync();
        return View(new PosPageViewModel { Customers = customers });
    }

    [HttpGet]
    public async Task<IActionResult> SearchMedicines(string? q)
    {
        var results = await _salesService.SearchMedicinesForSaleAsync(q);
        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([FromBody] PosCheckoutViewModel model)
    {
        var result = await _salesService.CheckoutPosAsync(model);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.ErrorMessage, errors = result.Errors });

        return Json(new
        {
            success = true,
            invoiceId = result.Data!.InvoiceId,
            total = result.Data.TotalAmount,
            itemCount = result.Data.ItemCount,
            receiptUrl = Url.Action(nameof(DownloadReceipt), new { id = result.Data.InvoiceId })
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _salesService.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> DownloadReceipt(int id)
    {
        var pdf = await _pdfService.GenerateSalesReceiptAsync(id);
        return pdf == null ? NotFound() : File(pdf, "application/pdf", $"SalesReceipt_{id}.pdf");
    }
}
